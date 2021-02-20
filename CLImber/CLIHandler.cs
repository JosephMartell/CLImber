using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CLImber
{
    public class CLIHandler
    {
        private readonly Dictionary<Type, IArgumentTypeConverter> _converters = new Dictionary<Type, IArgumentTypeConverter>();
        public CLIHandler RegisterTypeConverter<T>(IArgumentTypeConverter converter)
        {
            _converters[typeof(T)] = converter;
            return this;
        }

        protected Dictionary<Type, object> _resources = new Dictionary<Type, object>();
        public CLIHandler RegisterResource<T>(T resource)
        {
            _resources[typeof(T)] = resource;
            return this;
        }

        public CLIHandler()
        {
            _converters[typeof(int)] = new ArgToInt();
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
                var commandType = RetrieveTypeForCommand(args.First());
                object cmd = ConstructCmdType(commandType);
                InvokeHandlerMethod(commandType, cmd, args.Skip(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

        private void InvokeHandlerMethod(Type commandType, object cmd, IEnumerable<string> paramArgs)
        {
            var possibleHandlerMethods = AssemblySearcher.GetCommandMethods(commandType, paramArgs.Count());

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

            if (!_converters.ContainsKey(desiredType))
                throw new Exception($"Converter not registered for type {desiredType}");
    
            return _converters[desiredType]
                .ConvertArgument(parameter);
        }
    }
}
