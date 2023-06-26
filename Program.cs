using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace ipconfigcore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Network.DisplayIPNetworkInterfaces();
            }
            else if(args.Length == 1)
            {
                if (args.Contains("/all", StringComparer.InvariantCultureIgnoreCase))
                {
                    Network.DisplayIPNetworkInterfaces(true);
                }
                else if (args.Contains("/ips", StringComparer.InvariantCultureIgnoreCase))
                {
                    DisplaySummary();
                }

                else if (args.Contains("/help", StringComparer.InvariantCultureIgnoreCase))
                {
                    PrintUsage();
                }
                else
                {
                    Console.WriteLine("Unknown switch.");
                    PrintUsage();
                }
            }
        }

        private static void DisplaySummary()
        {
            Console.WriteLine("{0} IP configuration Summary", Network.GetOSPlatform());
            var ips = Network.GetAllIPAddresses();
            NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
            var hostname = Network.GetFQDN();
            Console.WriteLine("Hostname: {0}", hostname);
            foreach (IPAddress? ip in ips)
            {
                string tmp = ip.ToString();
                string interfacename = string.Empty;
                if (tmp.Contains('%'))
                {
                    string[] parts = tmp.Split('%');
                    int index = int.Parse(parts[1]) - 1;
                    interfacename = ifaces[index].Name + ' ' + ifaces[index].NetworkInterfaceType + ' ' + ifaces[index].GetIPProperties().DnsSuffix;
                }
                Console.WriteLine("{0}{1} Address: {2} {3}", ip.ToString().StartsWith("2001:") ? "Public " : " Local ", ip.ToString().Contains(':') ? "IPv6" : "IPv4", ip.ToString(), interfacename);
            }
            Console.WriteLine("Public IPv4 Address: {0}", Network.GetPublicIpAddress());

        }

        private static void PrintUsage()
        {
            string AppName = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0];
            Console.WriteLine(AppName);
            Console.WriteLine("/help - display usage.");
            Console.WriteLine("/ips - display list of active IP addresses.");
            Console.WriteLine("/all - display all interfaces");
        }
    }
}

