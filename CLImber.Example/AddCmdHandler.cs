using System;
namespace CLImber.Example
{
    [CommandClass("add", ShortDescription = "Add numbers together")]
    public class AddCmdHandler
    {
        [CommandHandler]
        public void AddNumbers(decimal num1, decimal num2)
        {
            Console.WriteLine($"Answer: {num1 + num2}");
        }
    }
}
