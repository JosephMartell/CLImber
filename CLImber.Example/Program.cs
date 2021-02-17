using System;
using System.Collections.Generic;
using CLImber;

namespace CLImber.Example
{
    class Program
    {
        static void CLImberConfig()
        {
            //register class as handler
            //Should pass the command (as a string), the class that handles the command, and a set of translators)
            _handler.RegisterCmdHandler<StatusCmdHandler>("status", new List<IArgumentTypeConverter>() { new ArgToInt() });
        }

        static CLIHandler _handler = new CLIHandler();
        static void Main(string[] args)
        {
            CLImberConfig();
            Console.WriteLine(_handler.Handle(new List<string>()));
            Console.WriteLine(_handler.Handle(new List<string>() { "5" }));

        }
    }

    public class StatusCmdHandler
        : CommandBase
    {
        public StatusCmdHandler()
        {
            Console.WriteLine("Default constructor");
        }

        public StatusCmdHandler(string arg1)
        {
            Console.WriteLine("String constructor: " + arg1);
        }

        public StatusCmdHandler(int arg2)
        {
            Console.Write("Int constructor: " + arg2);
        }
    }

    public class ArgToInt
        : IArgumentTypeConverter
    {
        object IArgumentTypeConverter.ConvertArgument(string arg)
        {
            return int.Parse(arg);
        }
    }
}
