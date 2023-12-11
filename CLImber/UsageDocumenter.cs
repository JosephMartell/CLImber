using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace CLImber
{
    //Here are the conventions that CLImber is going to use when describing CLIs:
    //Commands
    //     Commands are shown as plain-text words. 
    //
    //Arguments
    //     Arguments will be shown in either all CAPS or in angle brackets: <>
    //
    //Options
    //     Options will be delineated by starting either with "-" or "--"
    //
    //Optional elements will be shown in square brackets: []
    //Required elements will be shown in parenthesis: (). Elements that lack any decoration are also considered required.
    //Mutually exclusive options will be separated by a pipe: |
    //Elements that can be repeated will be followed by ellipsis: ...
    //These decisions are based off of the docopt conventions

    //Generated program documentation is going to take this format, by default:
    //program_name
    //
    //Commands:
    //command_name
    //Short description of what this does
    //  Usage:
    //      program_name command_name <argument> [options]
    //      program_name command_name <argument1> <argument2> [options]
    //      program_name command_name <argument1>...
    //
    //  Options
    //      --option1, -a                   Option 1 description
    //      --option2, -b                   Option 2 description
    //      --option3=<text>, -c=<text>     Option 3 description

    internal static class UsageDocumenter
    {
        //If <= 0 will ignore max width. 
        public static int MaxWidth { get; set; } = 120;
        private static string Indent(int levels)
        {
            if (levels <= 0)
                levels = 0;
            return new string(' ', 4 * levels);
        }
        public static void ShowUsageForAll(string programDescription = "")
        {
            var programName = Assembly.GetEntryAssembly().GetName();
            Console.WriteLine(programName.Name + " " + programName.Version);
            Console.WriteLine($"{programDescription}\n");
            Console.WriteLine("Commands:");

            var cmdTypes = AssemblySearcher
                .GetCommandClasses()
                .OrderBy(t => AssemblySearcher.GetCommandName(t));

            cmdTypes.ToList().ForEach(ShowUsageForType);
        }
        private static void ShowUsageForType(Type type)
        {
            var cmdName = AssemblySearcher.GetCommandName(type);
            Console.WriteLine($"{cmdName}");

            var desc = type.GetCustomAttributes<CommandClassAttribute>().First().ShortDescription;

            if (desc?.Length > 0)
            {
                Console.WriteLine($"{Indent(1)}{desc}");
            }
            
            PrintUsageDocumentation(type, cmdName);
            PrintOptionsDocumentation(type);
            Console.WriteLine("\n");
        }

        //TODO: This is a mess. Clean it up.
        private static void PrintUsageDocumentation(Type type, string cmdName)
        {
            bool hasOptions = AssemblySearcher.GetCommandOptions(type).Count() > 0;
            Console.WriteLine($"    usage:");

            List<(string usage, string desc)> methodDescriptions = new List<(string usage, string desc)>();
            foreach (var method in AssemblySearcher.GetCommandMethods(type))
            {
                var handlerDesc = method
                    .GetCustomAttributes<CommandHandlerAttribute>()
                    .First().ShortDescription;

                StringBuilder sb = new StringBuilder();
                sb.Append(cmdName).Append(" ");
                foreach (var param in method.GetParameters())
                {
                    sb.Append($"<{param.Name}>").Append(param.ParameterType.Name.Contains("[]") ?  "... " : " ");
                }

                if (hasOptions)
                {
                    sb.Append("[options] ");
                }
                methodDescriptions.Add((sb.ToString(), handlerDesc));
            }

            if (methodDescriptions.Count > 0)
            {
                var col1Width = methodDescriptions.Max(d => d.usage.Length) + Indent(2).Length + 2;
                foreach (var md in methodDescriptions)
                {
                    string usagePrint = Indent(2) + md.usage;
                    usagePrint = usagePrint.PadRight(col1Width, ' ');
                    string fullLine = $"{usagePrint}{md.desc}";
                    if ((MaxWidth > 0) && (fullLine.Length > MaxWidth))
                    {
                        var lastSpaceIndex = fullLine.LastIndexOf(' ', MaxWidth - 1, MaxWidth) + 1;
                        Console.WriteLine(fullLine.Substring(0, lastSpaceIndex));
                        int descIndent = usagePrint.Length;
                        do
                        {
                            fullLine = new string(' ', descIndent)+ fullLine.Substring(lastSpaceIndex);
                            if (fullLine.Length > MaxWidth)
                            {
                                Console.WriteLine(fullLine.Substring(0, MaxWidth));
                            }
                            else
                            {
                                Console.WriteLine(fullLine);
                            }
                        } while (fullLine.Length > MaxWidth);
                    }
                    else
                    {
                        Console.WriteLine($"{usagePrint}{md.desc}");
                    }
                }
            }
        }

        private static IEnumerable<string> SplitToLines(string text, int width)
        {
            List<string> lines = new List<string>();
            if (text.Length <= width)
            {
                lines.Add(text);
                return lines;
            }

            string line = string.Empty;
            while (text.Length > width)
            {
                line = text.Substring(0, text.LastIndexOf(' ', width - 1, width) + 1);
                lines.Add(line);
                text = text.Substring(width);
            }
            lines.Add(text);
            return lines;
        }

        private static void PrintOptionsDocumentation(Type type)
        {
            var optionList = from option in AssemblySearcher.GetCommandOptions(type)
                             from attributes in option.GetCustomAttributes<CommandOptionAttribute>()
                             select (attributes.Name, attributes.Abbreviation, attributes.Description, option.PropertyType);


            if (optionList.Count() > 0)
            {
                //6 spaces accounts for 4 spaces to print an abbreviated option (in the form of ', -o') or 4 spaces if no abbreviation is present, plus 2 hyphens to precede the full option.
                int longestItem = Indent(2).Length + optionList.Max(o => o.Name.Length + o.PropertyType.Name.Length + 3) + 6;

                var linesToPrint = from o in optionList
                                   select OptionStart(o, longestItem) + "  " + o.Description;

                Console.WriteLine("\n    options");
                linesToPrint.ToList().ForEach(Console.WriteLine);
            }
        }

        private static string OptionStart((string name, char abbreviation, string description, Type type) optionInfo, int width)
        {
            string start = Indent(2) + "--" + optionInfo.name;
            start += (optionInfo.abbreviation == '\0' ? "    " : ",-" + optionInfo.abbreviation);
            start += (optionInfo.type != typeof(bool) ? " =<" + optionInfo.type.Name + ">" : "").PadLeft(width - start.Length);

            return start;
        }
    }
}
