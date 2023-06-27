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
                else if (args.Contains("/license", StringComparer.InvariantCultureIgnoreCase))
                {
                    DisplayCopyright();
                }
                else if (args.Contains("/about", StringComparer.InvariantCultureIgnoreCase))
                {
                    DisplayAbout();
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
        private static void DisplayAbout()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string AppName = assembly.GetName().ToString().Split(',')[0] + ' ' + assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().FirstOrDefault().Version;
            // GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            var descriptionAttribute = assembly
         .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
         .OfType<AssemblyDescriptionAttribute>()
         .FirstOrDefault();

            if (descriptionAttribute != null)
            {
                Console.WriteLine(AppName);
                Console.WriteLine(descriptionAttribute.Description);
            }
        }
        private static void DisplayCopyright()
        {
            string licensetxt = @"BSD 2-Clause License

Copyright (c) 2023, Ari Ukkonen, ariukkonen

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ""AS IS""
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
";
            Console.WriteLine(licensetxt);
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
            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            Console.WriteLine(AppName+' '+ version);
            Console.WriteLine(AppName + " without a switch displays interfaces with less details.");
            Console.WriteLine("/about - displays About screen.");
            Console.WriteLine("/all - display all interfaces");
            Console.WriteLine("/help - display usage.");
            Console.WriteLine("/ips - display list of active IP addresses.");
            Console.WriteLine("/license - displays the license");
        }
    }
}

