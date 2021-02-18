using System;
using System.Net;
namespace CLImber.Example
{
    [CommandClass("add")]
    public class AddCmdHandler
    {
        [CommandHandler]
        public void AddDHCPProfile(string name)
        {
            Console.WriteLine("Add DHCP Profile named: " + name);


        }

        [CommandHandler]
        public void AddStaticProfile(string name, System.Net.IPAddress ip, System.Net.IPAddress subnet)
        {
            Console.WriteLine("Add static profile with only an ip (" + ip + ") and subnet (" + subnet + ")");


        }

        [CommandHandler]
        public void AddStaticProfile(string name, IPAddress ip, IPAddress subnet, IPAddress gateway)
        {
            Console.WriteLine($"Add static profile with ip, subnet, & gw: {ip}, {subnet}, {gateway}");


        }

        [CommandHandler]
        public void AddStaticProfile(string name, IPAddress ip, IPAddress subnet, IPAddress gateway, IPAddress DNS)
        {
            Console.WriteLine($"Add static profile with ip, subnet, gw, & DNS: {ip}, {subnet}, {gateway}, {DNS}");


        }
    }
}
