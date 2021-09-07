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
                   where (attributes != null && attributes.Length > 0)
                      && (method.GetParameters().Count() == argumentCount)
                   orderby method.GetParameters().Count() ascending
                   select method;
        }

        public static IEnumerable<PropertyInfo> GetCommandOptionPropertyByName(Type type, string optionName, bool ignoreCase = true)
        {
            var selectedOption = from op in type.GetProperties()
                                 let attribs = op.GetCustomAttributes<CommandOptionAttribute>()
                                 where (attribs.Count() > 0)
                                 from att in attribs
                                 where att.Name.Equals(optionName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                                    || att.Abbreviation.ToString().Equals(optionName, StringComparison.OrdinalIgnoreCase)
                                 select op;
            return selectedOption;
        }

    }
}
