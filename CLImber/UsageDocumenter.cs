using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace CLImber
{

    public static class UsageDocumenter
    {
        public static void ShowUsageForAll()
        {
            var cmdTypes = AssemblySearcher
                .GetCommandClasses()
                .OrderBy(t => AssemblySearcher.GetCommandName(t));

            //foreach type get the names of the parameters and create a printout
            foreach (var type in cmdTypes)
            {

                var cmdName = AssemblySearcher.GetCommandName(type);
                Console.WriteLine($"{cmdName} command");
                var desc = type.GetCustomAttributes(typeof(CommandClassAttribute), false).Cast<CommandClassAttribute>().First().ShortDescription;
                if (desc?.Length > 0)
                {
                    Console.WriteLine(desc);
                }

                Console.WriteLine($"usage:");

                foreach (var method in AssemblySearcher.GetCommandMethods(type))
                {
                    var handlerDesc = method.GetCustomAttributes(typeof(CommandHandlerAttribute), false).Cast<CommandHandlerAttribute>().First().ShortDescription;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(cmdName).Append(" ");
                    foreach (var param in method.GetParameters())
                    {
                        sb.Append(param.Name).Append(" ");
                    }
                    Console.WriteLine($"  {sb,-60} {handlerDesc}");
                }
                Console.WriteLine("\n");
            }
        }
    }
}
