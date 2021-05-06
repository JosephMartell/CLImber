using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CLImber
{
    public class CLIHandler
    {
        private readonly Dictionary<Type, Func<string, object>> _converterFuncs = new Dictionary<Type, Func<string, object>>();
        public CLIHandler RegisterTypeConverter<T>(Func<string, object> converterFunc)
        {
            _converterFuncs[typeof(T)] = converterFunc;
            return this;
        }

        private readonly Dictionary<Type, object> _resources = new Dictionary<Type, object>();
        public CLIHandler RegisterResource<T>(T resource)
        {
            _resources[typeof(T)] = resource;
            return this;
        }

        public CLIHandler()
        {
            _converterFuncs[typeof(int)] = (arg) => int.Parse(arg);
        }

        public void Handle(IEnumerable<string> args)
        {
            if (args.Count() <= 0)
            {
                UsageDocumenter.ShowUsageForAll();
                return;
            }

            try
            {
                string cmdArg = args.First();
                var options = args.Skip(1).Where(a => a.StartsWith("-"));
                var cmdArguments = args.Skip(1).Except(options);
                var commandType = RetrieveTypeForCommand(cmdArg);
                object cmd = ConstructCmdType(commandType);
                SetCommandOptions(cmd, options);
                InvokeHandlerMethod(cmd, cmdArguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SetCommandOptions(object commandObject, IEnumerable<string> passedOptions)
        {
            foreach (var option in passedOptions)
            {
                if (option.StartsWith("--"))
                {
                    ProcessFullOption(commandObject, option);
                    return;
                }

                //anything left must be a short option. Could be aggregated short option
                ProcessShortOptions(commandObject, option);
                
            }
        }

        private void ProcessShortOptions(object commandObject, string option)
        {
            var (Name, Value) = ParseOptionArgument(option);
            foreach (var letter in Name)
            {
                var optionProperty = AssemblySearcher
                    .GetCommandOptionPropertyByName(
                    commandObject.GetType(),
                    letter.ToString());

                ProcessOption(commandObject, optionProperty.First(), Value);
            }
        }

        private void ProcessFullOption(object commandObject, string option)
        {
            var (Name, Value) = ParseOptionArgument(option);

            var optionProperty = AssemblySearcher
                .GetCommandOptionPropertyByName(
                commandObject.GetType(),
                Name);

            ProcessOption(commandObject, optionProperty.First(), Value);
        }

        private void ProcessOption(object cmdObject, PropertyInfo propertyInfo, string value)
        {
            if (propertyInfo.PropertyType == typeof(bool))
            {
                propertyInfo.SetValue(cmdObject, true);
                return;
            }
            
            if (propertyInfo.PropertyType == typeof(string))
            {
                propertyInfo.SetValue(cmdObject, value);
                return;
            }
            
            if (_converterFuncs.ContainsKey(propertyInfo.PropertyType))
            {
                propertyInfo.SetValue(cmdObject, _converterFuncs[propertyInfo.PropertyType](value));
                return;
            }
        }

        private (string Name, string Value) ParseOptionArgument(string optionArgument)
        {
            var optionParts = optionArgument.Replace("-", "").Split(new char[] { '=' }, 2);
            return (optionParts.First(), optionParts.Count() > 1 ? optionParts.ElementAt(1) : string.Empty);
        }

        private Type RetrieveTypeForCommand(string command)
        {
            var possibleCommandHandlers = AssemblySearcher.GetCommandClasses(command);

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

        private void InvokeHandlerMethod(object cmd, IEnumerable<string> paramArgs)
        {
            var possibleHandlerMethods = AssemblySearcher.GetCommandMethods(cmd.GetType(), paramArgs.Count());

            //Temporary exception condition. Eventually we will just try multiple handlers starting with
            //the most specific signatures (the one that requires the most type conversion)
            if (possibleHandlerMethods.Count() > 1)
                throw new Exception("Multiple handlers exist with the same signature. Could not determine which handler to call.");

            if (possibleHandlerMethods.Count() < 1)
                throw new Exception("No handler was found for the given arguments.");

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
