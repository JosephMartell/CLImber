using System;
using System.Linq;

namespace CLImber.Example
{
    /// <summary>
    /// Subtract any quantity of numbers
    /// </summary>
    [CommandClass("sub", ShortDescription = "Subtract numbers")]
    public class Subtract
    {
        /// <summary>
        /// Subtract any number of numbers. Numbers are subtracted in the order they are entered.
        /// </summary>
        /// <param name="numbers">A list of numbers to subtract. The first number is considered the start. For example, the numbers 5 15 -9 would be interpreted as 5 - 4 - (-9) and return the value -1.</param>
        [CommandHandler]
        public void SubNumbers(decimal[] numbers)
        {
            Console.WriteLine($"Answer: {numbers.First() + numbers.Skip(1).Sum(s => -1 * s)}");
        }
    }
}
