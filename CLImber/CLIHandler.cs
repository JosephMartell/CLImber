using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace CLImber
{
    /// <summary>
    /// The primary class in the CLImber library. This class handles parsing command line arguments,
    /// determining which method to call, setting options, and converting arguments to the right type,
    /// and finally making the actual method call.
    /// </summary>
    public sealed class CLIHandler
    {
        private readonly Dictionary<Type, Func<string, object>> _converterFuncs = new Dictionary<Type, Func<string, object>>();

        /// <summary>
        /// Provide a function that converts from a command line argument (string) to a different type.
        /// This allows CLImber to work with strongly typed arguments for CommandHandler methods.
        /// </summary>
        /// <typeparam name="T">The type that will be passed to the handler method.</typeparam>
        /// <param name="converterFunc">The function that performs the conversion.</param>
        /// <returns>This instance of CLIHandler to facilitate fluent programming style.</returns>
        public CLIHandler RegisterTypeConverter<T>(Func<string, object> converterFunc)
        {
            _converterFuncs[typeof(T)] = converterFunc;
            return this;
        }

        private readonly Dictionary<Type, object> _resources = new Dictionary<Type, object>();
        /// <summary>
        /// A collection of resources that can be used for rudimentary dependancy injection.
        /// </summary>
        /// <typeparam name="T">The registered type of the resource. Generally should be an interface.</typeparam>
        /// <param name="resource">The instance/resource that will be provided if required by the CommandClass instance.</param>
        /// <returns>This instance of CLIHandler to facilitate fluent programming style.</returns>
        public CLIHandler RegisterResource<T>(T resource)
        {
            _resources[typeof(T)] = resource;
            return this;
        }

        /// <summary>
        /// If true, CLIHandler will ignore case when matching CLI commands. If false, the case of all CLI 
        /// commands must match the string provided in the CommandClass attribute exactly.
        /// </summary>
        public bool IgnoreCommandCase { get; set; }

        /// <summary>
        /// Provide a program description for program help and documentation.
        /// </summary>
        public string ProgramDescription { get; set; }

        /// <summary>
        /// Number of columns to use for printing text. Set to 0 or less to disable column restriction.
        /// </summary>
        public int ColumnCount { get; set; } = 0;
        public CLIHandler()
        {
            IgnoreCommandCase = true;
            _converterFuncs[typeof(int)] = (arg) => int.Parse(arg);
            _converterFuncs[typeof(float)] = (arg) => float.Parse(arg);
            _converterFuncs[typeof(double)] = (arg) => double.Parse(arg);
            _converterFuncs[typeof(decimal)] = (arg) => decimal.Parse(arg);
        }

        /// <summary>
        /// Parses command line arguments and then invoke the required handler methods.
        /// </summary>
        /// <param name="args">The command line arguments passed to your Main method.</param>
        public void Handle(IEnumerable<string> args)
        {
            if (args.Count() <= 0)
            {
                UsageDocumenter.ShowUsageForAll(ProgramDescription);
                return;
            }

            try
            {
                var argQ = new Queue<string>(args);
                string cmdArg = argQ.Dequeue();
                var options = args.Skip(1).Where(a => a.StartsWith("-"));
                var cmdArguments = args.Skip(1).Except(options);
                var commandType = RetrieveTypeForCommand(cmdArg);
                object cmd = ConstructCmdType(commandType);
                ParseOptions(cmd, argQ);
                InvokeHandlerMethod(cmd, cmdArguments);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void ParseOptions(object commandObject, Queue<string> argQ)
        {
            var passedOptions = new Queue<string>();
            while (argQ.Count > 0)
            {
                string arg = argQ.Dequeue();
                if (arg.StartsWith("-"))
                {
                    string options = arg;
                    string value = null;
                    if (arg.Contains("="))
                    {
                        var parts = arg.Split(new char[] { '=' }, 2);
                        options = parts[0];
                        value = parts[1];
                    }

                    if (options.StartsWith("--"))
                    {
                        SetFullOption(commandObject, argQ, options, value);
                    }
                    else if (options.StartsWith("-"))
                    {
                        SetAggregatedOptions(commandObject, argQ, options, value);
                    }
                }
                else
                {
                    passedOptions.Enqueue(arg);
                }
            }
        }
        private void SetFullOption(object commandObject, Queue<string> argQ, string options, string value)
        {
            var optionProperty = AssemblySearcher
                    .GetCommandOptionPropertyByName(
                        commandObject.GetType(),
                        options.Substring(2)).First();

            if (optionProperty.PropertyType == typeof(bool))
            {
                optionProperty.SetValue(commandObject, true);
            }
            else if (optionProperty.PropertyType == typeof(string))
            {
                optionProperty.SetValue(commandObject, value ?? argQ.Dequeue());
            }
            else if (_converterFuncs.ContainsKey(optionProperty.PropertyType))
            {
                optionProperty.SetValue(commandObject, _converterFuncs[optionProperty.PropertyType](value ?? argQ.Dequeue()));
            }
        }
        private void SetAggregatedOptions(object commandObject, Queue<string> argQ, string options, string value)
        {
            var argsRequired = 0;
            options = options.Substring(1);
            foreach (var option in options)
            {
                var optionProperty = AssemblySearcher
                        .GetCommandOptionPropertyByName(
                            commandObject.GetType(),
                            option.ToString()).First();

                if (optionProperty.PropertyType == typeof(bool))
                {
                    optionProperty.SetValue(commandObject, true);
                }
                else if (optionProperty.PropertyType == typeof(string))
                {
                    optionProperty.SetValue(commandObject, value ?? argQ.Dequeue());
                    argsRequired++;
                }
                else if (_converterFuncs.ContainsKey(optionProperty.PropertyType))
                {
                    optionProperty.SetValue(commandObject, _converterFuncs[optionProperty.PropertyType](value ?? argQ.Dequeue()));
                    argsRequired++;
                }

                if (argsRequired > 1)
                {
                    throw new ArgumentException("Aggregated options can only contain a single option that requires a value. Multiple were present.");
                }
            }

        }
        private Type RetrieveTypeForCommand(string command)
        {
            var possibleCommandHandlers = AssemblySearcher.GetCommandClasses(command, IgnoreCommandCase);

            if (possibleCommandHandlers.Count() > 1)
                throw new Exception($"More than one class assigned to handle command {command}");

            if (possibleCommandHandlers.Count() < 1)
                throw new Exception($"No handler registered for command: {command}");

            return possibleCommandHandlers.First();
        }
        private object ConstructCmdType(Type commandType)
        {
            if (commandType.GetConstructors().First().GetParameters().Count() < 1)
            {
                return commandType.GetConstructors().First().Invoke(new object[] { });
            }

            List<object> requiredResources = new List<object>();
            foreach (var param in commandType.GetConstructors().First().GetParameters())
            {
                if (!_resources.ContainsKey(param.ParameterType))
                {
                    throw new Exception($"Required resource is not registered: {param.ParameterType}");
                }
                requiredResources.Add(_resources[param.ParameterType]);
            }

            return commandType.GetConstructors().First().Invoke(requiredResources.ToArray());
        }

        //Handler method priority:
        // 1. Method parameter count exactly matches the supplied arguments
        //      a. If multiple methods qualify they will be tried in order of fewest string parameters to most
        // 2. Methods that have arrays as parameters
        //      a. If multiple methods qualify they will be tried in order of most non-array paremeters to least
        //      b. Preference will be given to methods with non-string array parameters.
        private void InvokeHandlerMethod(object cmd, IEnumerable<string> paramArgs)
        {
            var possibleHandlerMethods = AssemblySearcher.GetCommandMethods(cmd.GetType(), paramArgs.Count());

            if (possibleHandlerMethods.Count() > 0)
            {
                foreach (var possibleHandlerMethod in possibleHandlerMethods)
                {
                    //TODO: Do ANYTHING else besides swallowing the exception
                    try
                    {
                        List<object> convertedParamList = ConvertParams(possibleHandlerMethod.GetParameters(), paramArgs);
                        possibleHandlerMethod.Invoke(cmd, convertedParamList.ToArray());
                        return;
                    }
                    catch
                    {

                    }
                }
            }


            if (possibleHandlerMethods.Count() < 1)
            {
                var arrayHandlers = AssemblySearcher.GetCommandMethodsAcceptingArrays(cmd.GetType());
                if (arrayHandlers.Count() >= 1)
                {
                    foreach (var arrayHandler in arrayHandlers)
                    {
                        //TODO: Do ANYTHING else besides swallowing the exception
                        try
                        {
                            var nonArrayType = Type.GetType(arrayHandler.GetParameters().First().ParameterType.FullName.Replace("[]", ""));

                            Array typedArray = Array.CreateInstance(nonArrayType, paramArgs.Count());
                            for (int i = 0; i < paramArgs.Count(); i++)
                            {
                                typedArray.SetValue(ConvertParam(paramArgs.ElementAt(i), nonArrayType), i);
                            }
                            arrayHandler.Invoke(cmd, new object[] { typedArray });
                            return;
                        }
                        catch
                        {
                        }
                    }
                }
                throw new Exception("No handler was found for the given arguments.");
            }

            var handlerMethod = possibleHandlerMethods.First();

            List<object> convertedParams = ConvertParams(handlerMethod.GetParameters(), paramArgs);
            handlerMethod.Invoke(cmd, convertedParams.ToArray());
        }
        private List<object> ConvertParams(IEnumerable<ParameterInfo> methodParms, IEnumerable<string> paramArgs)
        {
            List<object> convertedParams = new List<object>();
            for (int i = 0; i < methodParms.Count(); i++)
            {
                convertedParams.Add(
                    ConvertParam(paramArgs.ElementAt(i),
                    methodParms.ElementAt(i).ParameterType));
            }

            return convertedParams;
        }
        private object ConvertParam(string parameter, Type desiredType)
        {
            if (desiredType == typeof(string))
                return parameter;

            if (!_converterFuncs.ContainsKey(desiredType))
                throw new Exception($"Converter not registered for type {desiredType}");

            return _converterFuncs[desiredType](parameter);
        }
    }
}
