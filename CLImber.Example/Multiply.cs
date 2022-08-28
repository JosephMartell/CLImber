using System;
using System.Linq;

namespace CLImber.Example
{
    [CommandClass("mul", ShortDescription = "Multiply numbers")]
    public class Multiply
    {
        [CommandHandler]
        public void MultiplyNumbers(decimal[] numbers)
        {
           Console.WriteLine($"Answer: {numbers.Aggregate(1.0m, (a, b) => a * b)}");
        }
    }
}
