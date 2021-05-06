using System;

namespace CLImber
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =true, Inherited = false)]
    public class CommandOptionAttribute
        : Attribute
    {
        public string Name { get; }

        public char Abbreviation { get; set; }
        public CommandOptionAttribute(string name)
        {
            Name = name;
        }
    }
}
