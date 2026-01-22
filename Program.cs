using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace ipconfigcore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool usenerdsymbols = false;
            bool fetchexternal = false;

            if (args.Length == 0)
            {
                Network.DisplayIPNetworkInterfaces(false,false);
            }
            else if(args.Length >= 1)
            {
                if (args.Contains("--nerd"))
                {
                    usenerdsymbols = true;
                    Console.OutputEncoding = System.Text.Encoding.UTF8;
                }
                if (args.Contains("--fetchip"))
                {
                    fetchexternal = true;
                }

                if (args.Contains("/all", StringComparer.InvariantCultureIgnoreCase) || args.Contains("-all", StringComparer.InvariantCultureIgnoreCase))
                {
                    Network.DisplayIPNetworkInterfaces(true, usenerdsymbols);
                }
                else if (args.Contains("/ips", StringComparer.InvariantCultureIgnoreCase) || args.Contains("-ips", StringComparer.InvariantCultureIgnoreCase))
                {
                    DisplaySummary(usenerdsymbols, fetchexternal);
                }
                else if (args.Contains("/license", StringComparer.InvariantCultureIgnoreCase) || args.Contains("-license", StringComparer.InvariantCultureIgnoreCase))
                {
                    DisplayCopyright();
                }
                else if (args.Contains("/about", StringComparer.InvariantCultureIgnoreCase) || args.Contains("-about", StringComparer.InvariantCultureIgnoreCase))
                {
                    DisplayAbout();
                }
                else if (args.Contains("/help", StringComparer.InvariantCultureIgnoreCase) || args.Contains("-help", StringComparer.InvariantCultureIgnoreCase))
                {
                    PrintUsage();
                }
                else
                {
                    if (usenerdsymbols)
                    {
                        Network.DisplayIPNetworkInterfaces(false, usenerdsymbols);
                    }
                    else
                    {
                        Console.WriteLine("Unknown switch.");
                        PrintUsage();
                            }
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

Copyright (c) 2025, Ari Ukkonen, ariukkonen

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

        private static void DisplaySummary(bool usenerdsymbols, bool fetchexternal)
        {
            string platform = Network.GetOSPlatform();
            string dotsymbol = usenerdsymbols ? "\ueb8a" : ".";
            string colonsymbol = usenerdsymbols ? "\u02D0" : ":";
            string twospaces = "  ";
            Console.WriteLine("{0} IP configuration Summary",usenerdsymbols ? Network.GetPlatformSymbol(platform) + " "+platform : platform);
            var ips = Network.GetAllIPAddresses();
            NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
            var hostname = Network.GetFQDN();
            Console.WriteLine("Hostname: {0}\n", hostname);
            if (usenerdsymbols)
            {
                InvertColours();
                UnderlineText();
                Console.Write("Address Type".PadLeft(20) + twospaces + "Address".PadRight(40) + twospaces + "Name".PadRight(35) + twospaces + "Interface Type".PadRight(20));
                UnderLineOff();
                Console.WriteLine();
                Console.ResetColor();
            }
            else
            {
                UnderlineText();
                Console.WriteLine("Address Type".PadLeft(20) + twospaces + "Address".PadRight(40) + twospaces + "Name".PadRight(35) + twospaces + "Interface Type".PadRight(20));
                UnderLineOff();
            }


            foreach (var item in ips)
            {

                    string interfacename = string.Empty;
                    interfacename = ifaces[item.Key].Name.PadRight(35).Substring(0,35) + twospaces + GetAdapterType(ifaces[item.Key].NetworkInterfaceType.ToString(), ifaces[item.Key].Name).PadRight(20) + twospaces + ifaces[item.Key].GetIPProperties().DnsSuffix;
                    foreach (var ip in item.Value)
                    {
                        Console.WriteLine("{0}{1} Address{5}{4}{2}{4}{3}", ip.ToString().StartsWith("2001:") ? "Public " : " Local ", ip.ToString().Contains(':') ? "IPv6" : "IPv4", ip.ToString().Replace(".", dotsymbol).Replace(":",colonsymbol).PadRight(40), interfacename, twospaces, colonsymbol);
                    }
            }
            if (fetchexternal)
            {
                Console.WriteLine();
                string publicip = Network.GetPublicIpAddressAsync().Result;
                Console.WriteLine("External IPv{0} Address{3}{2}{1}", publicip.Contains(":") ? "6" : "4", publicip.Replace(".", dotsymbol).Replace(":", colonsymbol), twospaces, colonsymbol);
            }

        }
        public static string GetAdapterType(string adaptertype, string name)
        {
            string type = string.Empty;
            if (adaptertype.Contains("Wireless"))
            {
                type = "Wireless LAN adapter";
            }
            else if (name.Contains("Bluetooth"))
            {
                type = "Bluetooth adapter";
            }
            else if (adaptertype.Contains("Ethernet"))
            {
                type = "Ethernet adapter";
            }
            else if (adaptertype.Contains("Tunnel") || name.Contains("utun") || adaptertype.Equals("53") || name.Contains("ipsec"))
            {
                type = "Tunnel adapter";
            }
            else
            {
                type = "Unknown adapter";
            }
            return type;
        }
        public static void InvertColours()
        {
            // Set inverted colors
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        public static void UnderlineText()
        {
            Console.Write("\u001b[4m");
        }

        public static void UnderLineOff()
        {
            Console.Write("\u001b[0m");
        }

        private static void PrintUsage()
        {
            string AppName = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0];
            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            Console.WriteLine(AppName+' '+ version);
            Console.WriteLine(AppName + " without a switch displays interfaces with less details.");
            Console.WriteLine("Switches:");
            Console.WriteLine("/-about - displays About screen.");
            Console.WriteLine("/-all - display all interfaces");
            Console.WriteLine("/-help - display usage.");
            Console.WriteLine("/-ips - display list of active IP addresses.");
            Console.WriteLine("/-license - displays the license");
            Console.WriteLine("Options:");
            Console.WriteLine("--nerd - displays information with nerd font symbols.");
            Console.WriteLine("--fetchip - fetches external IP address for ips switch.");
        }
    }
}

