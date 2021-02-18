using System;
using System.Collections.Generic;
using CLImber;

namespace CLImber.Example
{
    public interface ITestResource
    {
        string Resource { get; }
    }

    public class TestResource
        : ITestResource
    {
        public string Resource => "This is the test resource";
    }

    class Program
    {
        static void CLImberConfig()
        {
            _handler.RegisterResource<ITestResource>(new TestResource());
        }

        static CLIHandler _handler = new CLIHandler();
        static void Main(string[] args)
        {
            CLImberConfig();
            if (args.Length == 0)
            {
                Console.WriteLine(_handler.Handle(new string[] { "status", "Title" }));
                Console.WriteLine(_handler.Handle(new string[] { "add", "some other thing", "yet another thing" }));
                Console.WriteLine(_handler.Handle(new string[] { "add", "5" }));
            }
            else
            {
                Console.WriteLine(_handler.Handle(args));
            }
        }
    }

    [CommandClass("status")]
    public class StatusCmdHandler
    {
        [CommandHandler]
        public void NormalStatusReport()
        {
            Console.WriteLine("This method is invoked entirely via reflection");
        }

        //This is an example of explicitly designated type converter for individual arguments
        //[CommandHandler]
        //[DesignatedConverter(0, "stringToInt")]
        //[DesignatedConverter(1, "stringToInt")]
        //public void LineLimitedStatusReport(int lines, int columnLimit)
        //{

        //}

        [CommandHandler]
        public void StatusReportWithTitle(string title)
        {
            Console.WriteLine("Status report with title: " + title);
        }
            
        public StatusCmdHandler()
        {
            Console.WriteLine("Default constructor");
        }
    }

    [CommandClass("add")]
    public class AddCmdHandler
    {
        public AddCmdHandler(ITestResource resource)
        {
            Console.WriteLine("This constructor uses a resource: " + resource.Resource);
            Console.WriteLine("Add Command Handler Class Constructor");
        }

        [CommandHandler]
        public void AddSomething()
        {
            Console.WriteLine("This is the place to add something to a thing");
        }

        [CommandHandler]
        public void AnotherHandlerForAdd(string someString, string anotherString)
        {
            Console.WriteLine("Add handler with a parameter: " + someString + " " + anotherString);
        }

        [CommandHandler]
        public void HandlerThatTakesInt(int value)
        {
            Console.WriteLine("ADD handler that grabs an int: " + value);
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
