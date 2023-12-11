using System;
using System.Linq;

namespace CLImber.Example
{
    [CommandClass("add", ShortDescription = "Add numbers together")]
    internal class Add
    {
        [CommandOption("round", Abbreviation = 'r', Description = "If true, round answers to nearest whole number.")]
        public bool RoundAnswer { get; set; }

        [CommandHandler]
        public void AddNumbers(decimal[] numbers)
        {
            Console.WriteLine($"Answer: {numbers.Sum()}");
        }
    }
}
