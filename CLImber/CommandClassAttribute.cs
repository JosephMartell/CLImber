using System;

namespace CLImber
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandClassAttribute
        : System.Attribute
    {
        public string ShortDescription { get; set; }
        public string CommandName { get; }

        public CommandClassAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
