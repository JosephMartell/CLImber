using System;
using System.Linq;

namespace CLImber.Example
{
    /// <summary>
    /// Multiply any quantity of numbers
    /// </summary>
    [CommandClass("mul", ShortDescription = "Multiply numbers")]
    public class Multiply
    {
        /// <summary>
        /// Multiplies any quantity of numbers
        /// </summary>
        /// <param name="numbers">Supply numbers separated by spaces</param>
        [CommandHandler]
        public void MultiplyNumbers(decimal[] numbers)
        {
           Console.WriteLine($"Answer: {numbers.Aggregate(1.0m, (a, b) => a * b)}");
        }
    }
}
