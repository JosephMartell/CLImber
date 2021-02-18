using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CLImber
{
    public class CLIHandler
    {
        private Dictionary<Type, IArgumentTypeConverter> _converters = new Dictionary<Type, IArgumentTypeConverter>();
        public CLIHandler RegisterTypeConverter<T>(IArgumentTypeConverter converter)
        {
            _converters[typeof(T)] = converter;
            return this;
        }

        protected Dictionary<System.Type, object> _resources = new Dictionary<Type, object>();
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
                throw new Exception("No command specified"); //eventually this will just automatically display a list of all available commands

            var targetType = RetrieveTypeForCommand(args.First());
            object cmd = ConstructCmdType(targetType);
            InvokeHandlerMethod(targetType, cmd, args.Skip(1));
        }

        private Type RetrieveTypeForCommand(string command)
        {
            var typesWithAttr =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(CommandClassAttribute), true)
                where attributes != null && attributes.Length > 0
                select new { Type = t, Attributes = attributes.Cast<CommandClassAttribute>() };

            //then based on the command parsed from the args list, select the appropriate class and 
            //use reflection to instantiate and process.
            var selectedCmd = from t in typesWithAttr
                              let attributes = t.Attributes
                              from a in attributes
                              where a.CommandName == command
                              select t;

            if (selectedCmd.Count() > 1)
                throw new Exception($"More than one class assigned to handle command {command}");

            if (selectedCmd.Count() < 1)
                throw new Exception($"No handler registered for command: {command}");

            return selectedCmd.First().Type;
        }

        private object ConstructCmdType(Type targetType)
        {
            if (targetType.GetConstructors().First().GetParameters().Count() < 1)
            {
                return targetType.GetConstructors().First().Invoke(new object[] { });
            }

            List<object> requiredResources = new List<object>();
            foreach (var param in targetType.GetConstructors().First().GetParameters())
            {
                if (_resources.ContainsKey(param.ParameterType))
                {
                    requiredResources.Add(_resources[param.ParameterType]);
                }
                else
                {
                    throw new Exception($"Required resource is not registered: {param.ParameterType}");
                }
            }

            return targetType.GetConstructors().First().Invoke(requiredResources.ToArray());
        }

        //TODO: Review exceptions for converting to error messages back to user.
        private void InvokeHandlerMethod(Type targetType, object cmd, IEnumerable<string> paramArgs)
        {
            var possibleHandlerMethods =
                from m in targetType.GetMethods()
                let attributes = m.GetCustomAttributes(typeof(CommandHandlerAttribute), true)
                where (attributes != null && attributes.Length > 0)
                   && (m.GetParameters().Count() == paramArgs.Count())
                select m;

            //Temporary exception condition. Eventually we will just try multiple handlers starting with
            //the most specific signatures (the one that requires the most type conversion)
            if (possibleHandlerMethods.Count() > 1)
                throw new Exception("Multiple handlers exist with the same signature. Could not determine which handler to call.");

            if (possibleHandlerMethods.Count() < 1)
                throw new Exception("No handler was found for the given arguments.");

            if (possibleHandlerMethods.Count() == 1)
            {
                var handlerMethod = possibleHandlerMethods.First();
                try
                {
                    List<object> convertedParams = ConvertParams(handlerMethod.GetParameters(), paramArgs);
                    handlerMethod.Invoke(cmd, convertedParams.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not parse command. {ex.Message}");
                }
            }
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

            if (_converters.ContainsKey(desiredType))
            {
                return _converters[desiredType]
                    .ConvertArgument(parameter);
            }

            throw new Exception($"Converter not registered for type {desiredType}");
        }
    }
}
