using System;
namespace CLImber.Example
{
    [CommandClass("div", ShortDescription = "Divide numbers")]
    public class Divide
    {
        [CommandHandler]
        public void DivideNumbers(decimal num1, decimal num2)
        {
            Console.WriteLine($"Answer: {num1 / num2}");
        }
    }
}
