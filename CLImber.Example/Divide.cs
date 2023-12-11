using System;
namespace CLImber.Example
{
    /// <summary>
    /// Divide numbers
    /// </summary>
    [CommandClass("div", ShortDescription = "Divide numbers")]
    public class Divide
    {
        /// <summary>
        /// Divide numbers
        /// </summary>
        /// <param name="numerator">Numerator</param>
        /// <param name="divisor">Divisor</param>
        [CommandHandler]
        public void DivideNumbers(decimal numerator, decimal divisor)
        {
            Console.WriteLine($"Answer: {numerator / divisor}");
        }
    }
}
