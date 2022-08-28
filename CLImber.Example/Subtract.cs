using System;
using System.Linq;

namespace CLImber.Example
{
    [CommandClass("sub", ShortDescription = "Subtract numbers")]
    public class Subtract
    {
        [CommandHandler]
        public void SubNumbers(decimal[] numbers)
        {
            Console.WriteLine($"Answer: {numbers.First() + numbers.Skip(1).Sum(s => -1 * s)}");
        }
    }
}
