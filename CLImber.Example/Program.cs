﻿using System;
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
            _handler.RegisterResource<ITestResource>(new TestResource())
                .RegisterTypeConverter<System.Net.IPAddress>(new ArgToIP());
        }

        static CLIHandler _handler = new CLIHandler();
        static void Main(string[] args)
        {
            CLImberConfig();
            if (args.Length == 0)
            {
                _handler.Handle(new string[] { "status", "Title" });
                _handler.Handle(new string[] { "add", "some other thing", "yet another thing" });
                _handler.Handle(new string[] { "add", "5" });
            }
            else
            {
                _handler.Handle(args);
            }
        }
    }

    [CommandClass("network")]
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


    public class ArgToIP
        : IArgumentTypeConverter
    {
        public object ConvertArgument(string arg)
        {
            return System.Net.IPAddress.Parse(arg);
        }
    }
}
