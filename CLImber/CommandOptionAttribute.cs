using System;

namespace CLImber
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =true, Inherited = false)]
    public sealed class CommandOptionAttribute
        : Attribute
    {
        public string Name { get; }

        public char Abbreviation { get; set; }

        public string Description { get; set; }
        public CommandOptionAttribute(string name)
        {
            Name = name;
        }
    }
}
