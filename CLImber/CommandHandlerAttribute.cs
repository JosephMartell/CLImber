using System;

namespace CLImber
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHandlerAttribute
        : System.Attribute
    {
        public string ShortDescription { get; set; }
    }
}
