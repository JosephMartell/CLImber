using System;
using System.Linq;

namespace CLImber.Example
{
    [CommandClass("add", ShortDescription = "Add numbers together")]
    public class Add
    {
        [CommandHandler]
        public void AddNumbers(decimal num1, decimal num2)
        {
            Console.WriteLine($"Answer: {num1 + num2}");
        }

        [CommandHandler]
        public void AddNumbers(decimal[] numbers)
        {
            Console.WriteLine($"Answer: {numbers.Sum()}");
        }
    }
}
    