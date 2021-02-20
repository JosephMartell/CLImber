using System;

namespace CLImber
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandClassAttribute
        : System.Attribute
    {
        public string CommandName { get; }

        public CommandClassAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
