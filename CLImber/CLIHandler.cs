using System;
using System.Collections.Generic;
using System.Linq;

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

        public string Handle(IEnumerable<string> args)
        {
            if (args.Count() <= 0)
                return "No command supplied"; //eventually this will just automatically display a list of all available commands


            var targetType = RetrieveTypeForCommand(args.First());
            object cmd = ConstructCmdType(targetType);

            IEnumerable<string> paramArgs = args.Skip(1);
            //After instantiation, use the remaining arguments and translators to find the apporpriate
            //handler method.
            var possibleHandlerMethods =
                from m in targetType.GetMethods()
                let attributes = m.GetCustomAttributes(typeof(CommandHandlerAttribute), true)
                where (attributes != null && attributes.Length > 0)
                   && (m.GetParameters().Count() == paramArgs.Count())
                select m;

            if (possibleHandlerMethods.Count() > 1)
                throw new Exception("Multiple handlers exist with the same signature. Could not determine which handler to call.");

            if (possibleHandlerMethods.Count() < 1)
                throw new Exception("No handler was found for the given arguments.");

            if (possibleHandlerMethods.Count() == 1)
            {
                List<object> convertedParams = new List<object>();

                for (int i = 0; i < possibleHandlerMethods.First().GetParameters().Count(); i++)
                {
                    if (possibleHandlerMethods.First().GetParameters().ElementAt(i).ParameterType != typeof(string))
                    {
                        if (_converters.ContainsKey(possibleHandlerMethods.First().GetParameters().ElementAt(i).ParameterType))
                        {
                            convertedParams.Add(_converters[possibleHandlerMethods.First().GetParameters().ElementAt(i).ParameterType].ConvertArgument(paramArgs.ElementAt(i)));
                        }
                        else
                        {
                            throw new Exception("Converter for argument type is not registered. Parameter: " + possibleHandlerMethods.First().GetParameters().ElementAt(i).Name + " of type " + possibleHandlerMethods.First().GetParameters().ElementAt(i).ParameterType);
                        }
                    }
                    else
                    {
                        convertedParams.Add(paramArgs.ElementAt(i));
                    }
                    
                }
                possibleHandlerMethods.First().Invoke(cmd, convertedParams.ToArray());
            }


            return "constructor called";
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
    }

}
