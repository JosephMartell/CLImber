using System;

namespace CLImber
{
    [AttributeUsage(AttributeTargets.Class, Inherited =true)]
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
