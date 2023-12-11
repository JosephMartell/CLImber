using System;

namespace CLImber
{
    /// <summary>
    /// Used to denote a method in a CommandClass that is a valid handler for a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandHandlerAttribute
        : System.Attribute
    {
        /// <summary>
        /// Provide a short description that will provide a basic help message on the command line.
        /// </summary>
        public string ShortDescription { get; set; }
    }
}
