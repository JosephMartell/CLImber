using System;
namespace CLImber.Example
{
    [CommandClass("sub", ShortDescription = "Subtract numbers")]
    public class Subtract
    {
        [CommandHandler]
        public void SubNumbers(decimal num1, decimal num2)
        {
            Console.WriteLine($"Answer: {num1 - num2}");
        }
    }
}
