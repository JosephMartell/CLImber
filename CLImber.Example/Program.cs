using System;

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
            _handler.RegisterResource<ITestResource>(new TestResource())
                .RegisterTypeConverter<System.Net.IPAddress>(new ArgToIP());
        }

        static readonly CLIHandler _handler = new CLIHandler();
        static void Main(string[] args)
        {
            CLImberConfig();
            if (args.Length == 0)
            {
                _handler.Handle(new string[] { "status", "Title" });
                _handler.Handle(new string[] { "add", "test profile", "192.168.1.99", "255.255.255.0" });
                _handler.Handle(new string[] { });
            }
            else
            {
                _handler.Handle(args);
            }
        }
    }

    [CommandClass("network", ShortDescription = "Creates a network entry with an IP address")]
    public class NetworkCommand
    {
        [CommandHandler]
        public void WhatIsTheIP(System.Net.IPAddress ip)
        {
            Console.WriteLine("We were successfully passed an IP Address! " + ip);
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

        [CommandHandler(ShortDescription = "Prints a status report with the supplied title.")]
        public void StatusReportWithTitle(string title)
        {
            Console.WriteLine("Status report with title: " + title);
        }

        public StatusCmdHandler()
        {
            Console.WriteLine("Default constructor");
        }
    }


    public class ArgToIP
        : IArgumentTypeConverter
    {
        public object ConvertArgument(string arg)
        {
            return System.Net.IPAddress.Parse(arg);
        }
    }
}
