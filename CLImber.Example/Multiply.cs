using System;
namespace CLImber.Example
{
    [CommandClass("mul", ShortDescription = "Multiply numbers")]
    public class Multiply
    {
        [CommandHandler]
        public void MultiplyNumbers(decimal num1, decimal num2)
        {
            Console.WriteLine($"Answer: {num1 * num2}");
        }
    }
}
