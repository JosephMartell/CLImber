using System;
using System.Linq;

namespace CLImber.Example
{
    [CommandClass("add d", ShortDescription = "Add numbers together")]
    public class Add
    {
        [CommandHandler]
        public void AddNumbers(decimal[] numbers)
        {
            Console.WriteLine($"Answer: {numbers.Sum()}");
        }
    }
}
    