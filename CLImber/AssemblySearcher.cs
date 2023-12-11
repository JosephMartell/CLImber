using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace CLImber
{
    internal static class AssemblySearcher
    {
        public static IEnumerable<Type> GetCommandClasses()
        {
            var cmdTypes =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(CommandClassAttribute), true)
                where attributes != null && attributes.Count() > 0
                select t;

            return cmdTypes;
        }

        public static IEnumerable<Type> GetCommandClasses(string desiredCommand, bool ignoreCase = true)
        {
            var cmdTypes =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(CommandClassAttribute), true).Cast<CommandClassAttribute>()
                from attribute in attributes
                where attributes != null && attributes.Count() > 0
                  && attribute.CommandName.Equals(desiredCommand, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                select t;

            return cmdTypes;
        }

        public static string GetCommandName(Type type)
        {
            return type.GetCustomAttributes(typeof(CommandClassAttribute), false).Cast<CommandClassAttribute>().First().CommandName;
        }

        public static IEnumerable<MethodInfo> GetCommandMethods(Type type)
        {
            return from methods in type.GetMethods()
                   let attributes = methods.GetCustomAttributes(typeof(CommandHandlerAttribute), true)
                   where (attributes != null && attributes.Length > 0)
                   orderby methods.GetParameters().Count() ascending
                   select methods;
        }

        public static IEnumerable<MethodInfo> GetCommandMethods(Type type, int argumentCount)
        {
            return from method in type.GetMethods()
                   let attributes = method.GetCustomAttributes(typeof(CommandHandlerAttribute), true)
                   let parms = method.GetParameters()
                   where (attributes != null && attributes.Length > 0)
                      && (parms.Count() == argumentCount)
                      && (parms.Where(p => p.ParameterType.ToString().Contains("[]")).Count() == 0)
                   orderby parms.Count() ascending,
                           parms.Count(p => p.ParameterType == typeof(string)) ascending
                   select method;
        }

        public static IEnumerable<MethodInfo> GetCommandMethodsAcceptingArrays(Type type)
        {
            return from method in type.GetMethods()
                   let attributes = method.GetCustomAttributes(typeof(CommandHandlerAttribute), true)
                   let parms = method.GetParameters()
                   where (attributes != null && attributes.Length > 0)
                      && (parms.Count() > 0)
                      && (parms.First().ParameterType.ToString().Contains("[]"))
                   orderby parms.Count(p => p.ParameterType == typeof(string[])) ascending
                   select method;
        }

        public static IEnumerable<PropertyInfo> GetCommandOptions(Type type)
        {
            var selectedOptions = from op in type.GetProperties()
                                 let attribs = op.GetCustomAttributes<CommandOptionAttribute>()
                                 where (attribs.Count() > 0)
                                 from att in attribs
                                 select op;

            return selectedOptions;

        }
        public static IEnumerable<PropertyInfo> GetCommandOptionPropertyByName(Type type, string optionName)
        {
            var selectedOption = from op in type.GetProperties()
                                 let attribs = op.GetCustomAttributes<CommandOptionAttribute>()
                                 where (attribs.Count() > 0)
                                 from att in attribs
                                 where att.Name.Equals(optionName, StringComparison.CurrentCultureIgnoreCase )
                                    || att.Abbreviation.ToString().Equals(optionName, StringComparison.CurrentCultureIgnoreCase)
                                 select op;
            return selectedOption;
        }

    }
}
