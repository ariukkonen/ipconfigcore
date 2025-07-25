﻿using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ipconfigcore
{
    public static class Network
    {
        static List<string> exceptionpatterns = new List<string> { "fe80::1%1", "::1", "127.0.0.1" };
        static readonly HttpClient client = new HttpClient();

        public static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();
            if (!string.IsNullOrEmpty(domainName))
            {
                domainName = "." + domainName;
                if (!hostName.EndsWith(domainName))
                {
                    hostName += domainName;
                }
            }

            return hostName;
        }
        public static async Task<string> GetPublicIpAddressAsync(bool throwexception = false)
        {
            string publicIPAddress = string.Empty;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("curl");
            try
            {
                using HttpResponseMessage response = await client.GetAsync("http://ifconfig.me");
                response.EnsureSuccessStatusCode();
                publicIPAddress = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                if(throwexception) 
                {
                    throw;
                }
            }
            finally
            {
                publicIPAddress = publicIPAddress.Replace("\n", "");
            }

            return publicIPAddress;
        }
        public static string GetLocalIpAddress()
        {
            string localIp = string.Empty;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
            {
                if (!exceptionpatterns.Contains(ip.ToString()))
                {
                    localIp = ip.ToString();
                    break;
                }
            }
            return localIp;
        }

        public static List<IPAddress?> GetAllIP4Addresses()
        {
            List<IPAddress?> localIps = new List<IPAddress?>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
            {
                if (!exceptionpatterns.Contains(ip.ToString()))
                {
                    localIps.Add(ip);
                }
            }
            return localIps;
        }
        public static List<IPAddress?> GetAllIP6Addresses()
        {
            List<IPAddress?> ips = new List<IPAddress?>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetworkV6))
            {
                if (!exceptionpatterns.Contains(ip.ToString()))
                {
                    string tmp = ip.ToString();
                    if (tmp.Contains('%'))
                    {
                        string[] parts = tmp.Split('%');
                        int index = int.Parse(parts[1]) - 1;

                        if (index >= ifaces.Count())
                        {
                            continue;
                        }
                        if (ifaces[index].OperationalStatus.Equals(OperationalStatus.Up))
                        {
                            ips.Add(ip);
                        }
                    }
                    else
                    {
                        ips.Add(ip);
                    }
                }
            }
            return ips;
        }
        public static string FormatAsMacAddress(this string mac)
        {
            if (string.IsNullOrEmpty(mac))
            {
                return string.Empty;
            }

            string defaultmac = "00-00-00-00-00-00-00-E0";
            var regex = "^([a-fA-F0-9]{2}){6}$";
            try
            {
                string tmpmac = string.Join("-", Regex.Match(mac, regex).Groups[1].Captures.Select(x => x.Value));
                return string.IsNullOrEmpty(tmpmac) ? defaultmac : tmpmac;
            }
            catch (Exception)
            {
                return defaultmac;
            }
        }
        public static string GetOSPlatform() 
        {
            if (OperatingSystem.IsWindows())
                return "Windows";
            if (OperatingSystem.IsLinux()) 
            {
                return "Linux";
            }
            if (OperatingSystem.IsMacOS())
            {
                return "MacOS";
            }
            return "Unknown";
        }
        private static string ConvertBooltoYesNo(bool value)
        {
            string retval = "No";
            if (value)
            {
                retval = "Yes";
            }
            return retval;

        }
        public static void DisplayIPNetworkInterfaces(bool showalldetails = false, bool usenerdsymbols = false) 
        {
            string dotsymbol = usenerdsymbols ? "\uec07" : ".";
            string colonsymbol = usenerdsymbols ? "\ueb10" : ":";
            string endcap = usenerdsymbols ? "\ue0b4" : ":";
            string netbiosstatus = "Unknown";
            string lifeTimeFormat;
            string platform = GetOSPlatform();
            Dictionary<string, string> idcache = new Dictionary<string, string>();
            bool useidcache = false;
            Dictionary<string, int> versioninfo = new Dictionary<string, int>();
            GetOSVersion(versioninfo,platform);
            if (versioninfo["Major"].Equals(11) && platform.Equals("Windows"))
            {
                lifeTimeFormat = "MMMM d, yyyy h:mm:ss tt";
            }
            else
            {
                lifeTimeFormat = "dddd, MMMM d, yyyy h:mm:ss tt";
            }
            if (platform.Equals("Windows") || platform.Equals("MacOS")) 
            {
               LoadIDCachefromTempFolder(idcache);
               if(idcache.Count > 0)
               {
                    useidcache = true;
                }
            }
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            string platformlabel = usenerdsymbols ? GetPlatformSymbol(platform) +" "+platform : platform;
            Console.WriteLine();
            Console.WriteLine("{0} IP Configuration ", platformlabel);
            Console.WriteLine();
if(showalldetails) 
            {
                List<string> searchdomains = new List<string>();

#if OSX
            netbiosstatus = GetNetBiosStatusinOSX();
            string hostname = properties.HostName;
            string domain = string.Empty;
            if (hostname.Contains('.'))
            {
                string[] parts = properties.HostName.Split('.');
                hostname = parts[0];
                domain = parts[1];
            }
                searchdomains.Add(domain);
                foreach (NetworkInterface adapter in nics)
                {
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    if (!string.IsNullOrEmpty(adapterProperties.DnsSuffix))
                    {
                        if (!searchdomains.Contains(adapterProperties.DnsSuffix))
                        {
                            searchdomains.Add(adapterProperties.DnsSuffix);
                        }
                    }
                }

            Console.WriteLine("   Host Name {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, hostname);
            Console.WriteLine("   Primary Dns Suffix  {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol, colonsymbol, domain);
            Console.WriteLine("   Node Type {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, properties.NodeType);
            Console.WriteLine("   IP Routing Enabled{0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, properties.GetIPv4GlobalStatistics().NumberOfRoutes > 0 ? "Yes" : "No");
#else
            Console.WriteLine("   Host Name {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, properties.HostName);
            Console.WriteLine("   Primary Dns Suffix  {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, properties.DomainName);
            Console.WriteLine("   Node Type {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, properties.NodeType);
            Console.WriteLine("   IP Routing Enabled{0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, properties.GetIPv4GlobalStatistics().NumberOfRoutes > 0 ? "Yes" : "No");
#if Windows
            if (platform.Equals("Windows")) 
            {
                Console.WriteLine("   WINS Proxy Enabled{0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ConvertBooltoYesNo(properties.IsWinsProxy));
                    if (idcache.ContainsKey("NETBIOS"))
                    {
                        netbiosstatus = idcache["NETBIOS"];
                    }
                    else
                    {
                        useidcache = false;
                        netbiosstatus = GetNetBiosStatusinWindows();
                        idcache.Add("NETBIOS", netbiosstatus);
                    }
            }
#endif
            searchdomains.Add(properties.DomainName);
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                if (!string.IsNullOrEmpty(adapterProperties.DnsSuffix))
                {
                    if (!searchdomains.Contains(adapterProperties.DnsSuffix))
                    {
                        searchdomains.Add(adapterProperties.DnsSuffix);
                    }
                }
            }
#endif
            if (!searchdomains.Count.Equals(0))
            {
                    Console.WriteLine("   DNS Suffix Search List{0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, searchdomains[0]);
                for (int i = 1; i < searchdomains.Count; i++)
                {
                    Console.WriteLine("                                       {0}", searchdomains[i]);
                }

            }

        }
#if OSX
            string duid = string.Empty;
            if (idcache.ContainsKey("DUID"))
            {
                duid = idcache["DUID"];
            }
            else 
            {
                useidcache = false;
                duid = GetDUIDforMacOS();
                idcache.Add("DUID", duid);
            }
            
#elif Windows
            string DUID = string.Empty;
            if (platform.Equals("Windows")) 
            {
                if (idcache.ContainsKey("DUID"))
                {
                    DUID = idcache["DUID"];
                }
                else 
                {
                    useidcache = false;
                    DUID = GetDUIDforWindows();
                    idcache.Add("DUID", DUID);
                }
            }
#endif

            foreach (NetworkInterface adapter in nics)
                {
                // Only display information for interfaces that support IPv4.
                if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false && (adapter.Supports(NetworkInterfaceComponent.IPv6) == false))
                {
                    continue;
                }
                bool isloopback = false;
                foreach (UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (exceptionpatterns.Contains(ip.Address.ToString()))
                    {
                        isloopback = true;
                    }
                }
                if (isloopback)
                {
                    continue;
                }
                string macaddress = adapter.GetPhysicalAddress().ToString();

                string adaptertitle = GetAdapterTitle(adapter.NetworkInterfaceType.ToString(), adapter.Name);
                string startcap = GetStartCap(adaptertitle,usenerdsymbols);
                Console.WriteLine();
                WriteTitle(startcap, adaptertitle, endcap, usenerdsymbols);
                Console.WriteLine();

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                // Try to get the IPv4 interface properties.
                IPv4InterfaceProperties p = null;
                try
                {
                    // Try to get the IPv4 interface properties.
                    p = adapterProperties.GetIPv4Properties();
                }
                catch (Exception)
                {
                }

                IPv6InterfaceProperties p6 = null;
                try
                {
                    // Try to get the IPv6 interface properties.
                    p6 = adapterProperties.GetIPv6Properties();
                }
                catch (Exception) 
                { 
                }
                if (p == null && p6 == null)
                {
                    Console.WriteLine("No IPv4 or IPV6 information is available for this interface.");
                    Console.WriteLine();
                    continue;
                }
                if (adapter.OperationalStatus.Equals(OperationalStatus.Up))
                {
#if OSX
                    string macOSdhcpaddress = string.Empty;
                    if(p != null)
                    {
                        macOSdhcpaddress = GetDHCPServeronMacOS(adapter.Id);
                    }
#endif

                    Console.WriteLine("   Connection-specific DNS Suffix  {0} {1} {2}", dotsymbol, colonsymbol, adapterProperties.DnsSuffix);

                    if (showalldetails)
                    {
                        Console.WriteLine("   Description {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, adapter.Description);
                        Console.WriteLine("   Physical Address{0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, macaddress.FormatAsMacAddress());
                    }
                    if(showalldetails) 
                    {
#if Windows
                    if(platform.Equals("Windows"))
                        Console.WriteLine("   DHCP Enabled{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ConvertBooltoYesNo(p.IsDhcpEnabled));
#elif OSX

                    if (!string.IsNullOrEmpty(macOSdhcpaddress))
                    {
                        Console.WriteLine("   DHCP Enabled{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, "Yes");
                    }
                    else
                    {
                        Console.WriteLine("   DHCP Enabled{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, "No");
                    }
#endif
                    }
                    string addresspreference = string.Empty;
                    if (p6 != null)
                    {
                        if (showalldetails)
                        {
                            Console.WriteLine("   Autoconfiguration Enabled {0} {0} {0} {0} {1} Yes",dotsymbol,colonsymbol);
                        }
                        int ipv6count = 0;
                        // Display the IPv6 specific data.
                        foreach (UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                            {
                                if (showalldetails)
                                {
                                    addresspreference = "(Preferred)";
                                }
                                if (ipv6count.Equals(0) && ip.Address.ToString().StartsWith("2001:"))
                                {
                                    Console.WriteLine("   IPv6 Address{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ip.Address.ToString()+ addresspreference);
                                }
                                else if (ipv6count > 0 && ip.Address.ToString().StartsWith("2001:"))
                                {
                                    Console.WriteLine("   Temporary IPv6 Address{0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ip.Address.ToString() + addresspreference);
                                }
                                else
                                {
                                    Console.WriteLine("   Link-local IPv6 Address {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ip.Address.ToString() + addresspreference);
                                }
                                if (ip.Address.ToString().StartsWith("2001:"))
                                    ipv6count++;
                            }
                        }
                    }
                    else
                    {
                        if(showalldetails)
                        {
                            Console.WriteLine("   Autoconfiguration Enabled {0} {0} {0} {0} {1} False", dotsymbol, colonsymbol); 
                        }
                    }
                    if (p != null)
                    {
                        // Display the IPv4 specific data.
                        foreach (UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                if (showalldetails)
                                {
                                    addresspreference = "(Preferred)";
                                }
                                Console.WriteLine("   IPv4 Address{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ip.Address.ToString() + addresspreference);
                                Console.WriteLine("   Subnet Mask {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ip.IPv4Mask.ToString());
#if Windows
                                if (platform.Equals("Windows")) 
                                {
                                    if (showalldetails)
                                    {
                                        DateTime when;
                                        when = DateTime.Now + (TimeSpan.FromSeconds(ip.AddressValidLifetime) - TimeSpan.FromSeconds(ip.DhcpLeaseLifetime));
                                        if(when.Year <= DateTime.Now.Year)
                                        {
                                            Console.WriteLine("   Lease Obtained{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, when.ToString(lifeTimeFormat, CultureInfo.CurrentCulture));
                                            when = DateTime.Now + TimeSpan.FromSeconds(ip.AddressPreferredLifetime);
                                            Console.WriteLine("   Lease Expires {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, when.ToString(lifeTimeFormat, CultureInfo.CurrentCulture));
                                        }
                                    }
                                }
#elif OSX
                                 if (showalldetails)
                                {
                                    KeyValuePair<string, string> leaseinfo = GetLeaseInfoonMacOS(adapter.Name);
                                    if (!leaseinfo.Equals(default(KeyValuePair<string, string>)))
                                    {
                                        string inputformat = "G";
                                        DateTime when = DateTime.ParseExact(leaseinfo.Key, inputformat, DateTimeFormatInfo.InvariantInfo);
                                        Console.WriteLine("   Lease Obtained{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, when.ToString(lifeTimeFormat, CultureInfo.CurrentCulture));
                                        when = DateTime.ParseExact(leaseinfo.Value, inputformat, DateTimeFormatInfo.InvariantInfo);
                                        Console.WriteLine("   Lease Expires {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, when.ToString(lifeTimeFormat, CultureInfo.CurrentCulture));
                                    }
                                }
#endif
                            }
                        }
                        if (adapterProperties.GatewayAddresses.Count > 0)
                        {
                            Console.WriteLine("   Default Gateway {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, adapterProperties.GatewayAddresses[0].Address.ToString());
                            int i = 1;
                            while (i < adapterProperties.GatewayAddresses.Count)
                            {
                                Console.WriteLine("                                       {0}", adapterProperties.GatewayAddresses[i].Address.ToString());
                                i++;
                            }
                        }
                        else 
                        {
                            Console.WriteLine("   Default Gateway {0} {0} {0} {0} {0} {0} {0} {0} {0} {1}",dotsymbol, colonsymbol);
                        }
                        if (showalldetails)
                        {

#if !OSX
                            foreach (var dhcpaddress in adapterProperties.DhcpServerAddresses)
                            {
                                Console.WriteLine("   DHCP Server {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, dhcpaddress);
                            }
                            if(!string.IsNullOrEmpty(macaddress))
                            {
                                if (versioninfo["Major"] >= 10 && platform.Equals("Windows"))
                                {
                                    if (!idcache.ContainsKey(adapter.Id)) 
                                    {
                                        useidcache = false;
                                    }
                                    Console.WriteLine("   DHCPv6 IAID {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, useidcache ? idcache[adapter.Id] : GetIAIDforWindow(adapter.Id, idcache));
                                }
                                else 
                                {
                                    Console.WriteLine("   DHCPv6 IAID {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, "IAID retrieval Unsupported");

                                }
#if !Linux
                                Console.WriteLine("   DHCPv6 Client DUID{0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, DUID);
#endif
                            }
#else
                            if (!string.IsNullOrEmpty(macOSdhcpaddress))
                            {
                            Console.Write("   DHCP Server {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, macOSdhcpaddress);
                            }
                            if(!string.IsNullOrEmpty(macaddress))
                            {
                                if (!idcache.ContainsKey(macaddress)) 
                                {
                                    useidcache = false;
                                }
                                Console.WriteLine("   DHCPv6 IAID {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, useidcache ? idcache[macaddress] : GetIAIDforMacOS(macaddress, idcache));
                                Console.WriteLine("   DHCPv6 Client DUID{0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, duid);
                            }

#endif
                            IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                            List<IPAddress>  filtereddnslist =  new List<IPAddress>();
                            foreach (var dnsserver in dnsServers)
                            {
                                if (!dnsserver.ToString().Contains("fec0:0:0:ffff")) 
                                {
                                    filtereddnslist.Add(dnsserver);
                                }
                            }
                            if (filtereddnslist.Count > 0)
                            {
                                Console.WriteLine("   DNS Servers {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, filtereddnslist[0].ToString());
                                int i = 1;
                                while (i < filtereddnslist.Count)
                                {

                                Console.WriteLine("                                       {0}", filtereddnslist[i].ToString());
                                    i++;
                                }
                            }
                            Console.WriteLine("   NetBIOS over Tcpip{0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, netbiosstatus);
                        }
                    }
                }
                else 
                {
                        Console.WriteLine("   Media State {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} Media disconnected", dotsymbol, colonsymbol);
                        Console.WriteLine("   Connection-specific DNS Suffix  {0} {1} {2}", dotsymbol, colonsymbol, adapterProperties.DnsSuffix);

                    if (showalldetails) 
                    {
                        Console.WriteLine("   Description {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, adapter.Description);
                        Console.WriteLine("   Physical Address{0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, macaddress.FormatAsMacAddress());

#if Windows
                        if (p != null) 
                        { 
                           Console.WriteLine("   DHCP Enabled{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, ConvertBooltoYesNo(p.IsDhcpEnabled));
                        }
                        else
                        {
                        	Console.WriteLine("   DHCP Enabled{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}", dotsymbol, colonsymbol, "No");
                        }
#endif
#if OSX
                        Console.WriteLine("   DHCP Enabled{0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {0} {1} {2}",dotsymbol,colonsymbol, "No");
#endif
                        if (p6 != null)
                        {
                            Console.WriteLine("   Autoconfiguration Enabled {0} {0} {0} {0} {1} Yes", dotsymbol, colonsymbol);

                        }
                        else
                        {
                            Console.WriteLine("   Autoconfiguration Enabled {0} {0} {0} {0} {1} No", dotsymbol, colonsymbol);
                        }
                    }
                }
            }
            if(!useidcache && idcache.Count > 0)
            {
                WriteIDCacheToTempFolder(idcache);
            }
        }

        private static void WriteTitle(string startcap, string adaptertitle, string endcap, bool usenerdsymbols)
        {
            string platform = GetOSPlatform();
            ConsoleColor originalForeground = Console.ForegroundColor;
            ConsoleColor originalBackground = Console.BackgroundColor;
            if (usenerdsymbols)
            {

                // Set inverted colors
                if (platform == "Windows")
                {
                    Console.ForegroundColor = originalBackground;
                    Console.BackgroundColor = originalForeground;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.Write(startcap);
                Console.Write(adaptertitle+' ');
                // Reset to the original colors
                Console.ResetColor();
                Console.WriteLine(endcap);
            }
            else
            {
                Console.WriteLine("{0}{1}{2}", startcap, adaptertitle, endcap);
            }

        }

        private static string GetStartCap(string adaptertitle, bool usenerdsymbols)
        {
            string standardcap = usenerdsymbols ? " \ue0b6 " : "";
            string ethernetcap = " \U000F0201 ";
            string wificap = " \U000F05a9 ";
            string vpncap = " \U000F0582 ";
            string btcap = " \U000f00af "; 
            string retval = string.Empty;

            if (adaptertitle.Contains("Bluetooth") && usenerdsymbols)
            {
                retval = btcap;
            }
            else if (adaptertitle.Contains("Ethernet") && usenerdsymbols && string.IsNullOrWhiteSpace(retval))
            {
                retval = ethernetcap;
            }
            else if (adaptertitle.Contains("Wireless") && usenerdsymbols)
            {
                retval = wificap;
            }
            else if (adaptertitle.Contains("Tunnel") && usenerdsymbols)

            {
                retval = vpncap;
            }
            else
            {
                retval = standardcap;
            }

            return retval;
        }

        public static string GetPlatformSymbol(string platform)
        {
            string symbol = string.Empty;
            switch (platform)
            {
                case "MacOS":
                    symbol = "\ue711";
                    break;
                case "Windows":
                    symbol = "\ue70f";
                    break;
                case "Linux":
                    symbol = "\ue712";
                    break;
            }
            return symbol;
        }

        private static void WriteIDCacheToTempFolder(Dictionary<string, string> idcache)
        {
            string tempPath = Path.GetTempPath();
            string tempfilename = Path.Combine(tempPath, "ipconfig_cache.txt");

            using (StreamWriter file = new StreamWriter(tempfilename))
                foreach (var entry in idcache)
                    file.WriteLine("{0}={1}", entry.Key, entry.Value);
        }

        private static void LoadIDCachefromTempFolder(Dictionary<string, string> idcache)
        {
            string tempPath = Path.GetTempPath();
            string tempfilename = Path.Combine(tempPath, "ipconfig_cache.txt");
            if (File.Exists(tempfilename))
            {
                var info = new FileInfo(tempfilename);
                info.Refresh();
                DateTime modifiedon = info.LastWriteTime;
                DateTime now = DateTime.Now;
                if(modifiedon < now.AddMinutes(-30))
                {
                    File.Delete(tempfilename);
                    return;
                }
                using(StreamReader sr = new StreamReader(tempfilename))
{
                    string _line;
                    while ((_line = sr.ReadLine()) != null)
                    {
                        string[] keyvalue = _line.Split('=');
                        if (keyvalue.Length == 2)
                        {
                            idcache.Add(keyvalue[0], keyvalue[1]);
                        }
                    }
                }
            }
        }

        public static void GetOSVersion(Dictionary<string,int>  info, string platform)
        {
            info.Add("Major", Environment.OSVersion.Version.Major);
            info.Add("Minor", Environment.OSVersion.Version.Minor);
            info.Add("Build", Environment.OSVersion.Version.Build);
            if (platform.Equals("Windows"))
            {
                if (info["Major"].Equals(10) && info["Build"] >= 22000)
                    info["Major"] = 11;
            }
        }
        private static string GetIAIDforMacOS(string physicalAddress, Dictionary<string,string> idcache)
        {
            string retval = string.Empty;
            if (!string.IsNullOrEmpty(physicalAddress))
            {
                int day = DateTime.Now.Day;
                string hex = string.Format("{0:x}", day);
                hex = hex.Length.Equals(2) ? hex : "0" + hex;
                string tmpstr = hex + physicalAddress.Substring(0, 6);
                retval = int.Parse(tmpstr, NumberStyles.HexNumber).ToString();
                if (!idcache.ContainsKey(physicalAddress))
                {
                    idcache.Add(physicalAddress, retval);
                }
            }
            return retval;
        }

        private static string GetIAIDforWindow(string id,Dictionary<string,string> iaidcache) 
        {
            string returnvalue = "";
            string result = string.Empty;
            try
            {
                string strWorkPath = System.AppContext.BaseDirectory;
                string strps1FilePath = System.IO.Path.Combine(strWorkPath, "getdhcpv6iaid.ps1");
                var command = "powershell";
                var arguments = string.Format(" {0} {1}", strps1FilePath, id.Replace("{", "").Replace("}", ""));
                var processInfo = new ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true

                };

                Process process = Process.Start(processInfo);   // Start that process.
                while (!process.StandardOutput.EndOfStream)
                {
                    result = process.StandardOutput.ReadToEnd();
                }
                process.WaitForExit();
                returnvalue = result.Trim().ReplaceLineEndings();
                if(!iaidcache.ContainsKey(id))
                {
                    iaidcache.Add(id, returnvalue);
                }
            }
            catch (Exception)
            {
                returnvalue = "IAID retrieval Unsupported";
            }
            return returnvalue;
        }
        private static string GetDUIDforWindows() 
        {
            string returnvalue = "";
            string result = string.Empty;
            var command = "powershell";
            var arguments = " (Get-ItemProperty HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip6\\Parameters).Dhcpv6DUID";
            var processInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true

            };

            Process process = Process.Start(processInfo);   // Start that process.
            while (!process.StandardOutput.EndOfStream)
            {
                result = process.StandardOutput.ReadToEnd();
            }
            process.WaitForExit();
            List<string> list = new List<string>(); 
            StringReader strReader = new StringReader(result);
            while (true)
            {
                string aLine = string.Empty;
                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    list.Add(NumbertoHex(aLine.Trim().ReplaceLineEndings()));
                }
                else
                {
                    break;
                }
                returnvalue = String.Join("-", list).ToUpper();
            }
            return returnvalue;
        }
        private static string NumbertoHex(string value)
        {
            string hex = string.Format("{0:x}", int.Parse(value));
            return hex.Length.Equals(2) ? hex : "0" + hex;
        }

        private static string GetNetBiosStatusinOSX()
        {
            string returnvalue = "Disabled";
            string result = string.Empty;
            var command = "defaults";
            var arguments = " read /Library/Preferences/SystemConfiguration/com.apple.smb.server NetBIOSName";
            var processInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true

            };

            Process process = Process.Start(processInfo);   // Start that process.
            while (!process.StandardOutput.EndOfStream)
            {
                result = process.StandardOutput.ReadToEnd();
            }
            process.WaitForExit();
            if (!string.IsNullOrEmpty(result))
            {
                returnvalue = "Enabled";
            }
            return returnvalue;

        }

        private static string GetAdapterTitle(string adaptertype, string name)
        {
            string title = string.Empty;
            if (adaptertype.Contains("Wireless"))
            {
                title = "Wireless LAN adapter" + ' ' + name;
            }
            else if (name.Contains("Bluetooth"))
            {
                title = "Bluetooth adapter" + ' ' + name;
            }
            else if (adaptertype.Contains("Ethernet"))
            {
                title = "Ethernet adapter" + ' ' + name;
            }
            else if (adaptertype.Contains("Tunnel") || name.Contains("utun") || adaptertype.Equals("53"))
            {
                title = "Tunnel adapter" + ' ' + name;
            }
            else
            {
                title = "Unknown adapter" + ' ' + name;
            }
            return title;
        }

        private static string GetNetBiosStatusinWindows()
        {
            string returnvalue = "Disabled";
            string result = string.Empty;
            var command = "powershell";
            var arguments = " Get-WmiObject Win32_ComputerSystem";
            var processInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true

            };

            Process process = Process.Start(processInfo);   // Start that process.
            while (!process.StandardOutput.EndOfStream)
            {
                result = process.StandardOutput.ReadToEnd();
            }
            process.WaitForExit();
            if (result.Contains("Domain")) 
            {
                string aLine = string.Empty;
                string netbiosdomain = string.Empty;
                StringReader strReader = new StringReader(result);
                while (true)
                {
                    aLine = strReader.ReadLine();
                    if (aLine != null)
                    {
                        if (aLine.Contains("Domain              :"))
                        {
                            netbiosdomain = aLine.Replace("Domain              :", "").Trim();

                        }
                        if (!string.IsNullOrEmpty(netbiosdomain))
                        {
                            returnvalue = "Enabled";
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return returnvalue;
        }
        public static string GetDHCPServeronMacOS(string interfacename)
        {
            string result = string.Empty;
            var command = "ipconfig";
            var arguments = string.Format("getoption {0} server_identifier", interfacename);
            var processInfo = new ProcessStartInfo()
            {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true

                };

                Process process = Process.Start(processInfo);   // Start that process.
                while (!process.StandardOutput.EndOfStream)
                        { 
                   result = process.StandardOutput.ReadToEnd();
                }
                process.WaitForExit();
            return result;
        }
        // system_profiler SPHardwareDataType
        // "   DHCPv6 Client DUID. . . . . . . . : 00-01-00-01-29-02-8A-28-80-D2-1D-DC-9C-F9 == Hardware UUID
        public static string GetDUIDforMacOS()
        {
            // ipconfig getsummary en0
            string returnval = string.Empty;
            string result = string.Empty;
            var command = "system_profiler";
            var arguments = " SPHardwareDataType";
            var processInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true

            };

            Process process = Process.Start(processInfo);   // Start that process.
            while (!process.StandardOutput.EndOfStream)
            {
                result = process.StandardOutput.ReadToEnd();
            }
            process.WaitForExit();
            if (result.Contains("Hardware UUID"))
            {
                string aLine;
                string DUID = string.Empty;

                StringReader strReader = new StringReader(result);
                while (true)
                {
                    aLine = strReader.ReadLine();
                    if (aLine != null)
                    {
                        if (aLine.Contains("Hardware UUID:"))
                        {
                            DUID = aLine.Replace("Hardware UUID:", "").Replace("-","").Replace('\n', ' ').Trim();
                            DUID = Regex.Replace(String.Format("{0:X8}", DUID),"([0-9A-F]{2})(?!$)","$1-");
                            
                        }
                        if (!string.IsNullOrEmpty(DUID))
                        {
                            returnval = DUID;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                }
            }
            return returnval;
        }

        public static KeyValuePair<string,string> GetLeaseInfoonMacOS(string interfacename)
        {
            KeyValuePair<string, string> returnval = new KeyValuePair<string, string>();
            try
            {
                string result = string.Empty;
                var command = "ipconfig";
                var arguments = string.Format(" getsummary {0}", interfacename);
                var processInfo = new ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                Process process = Process.Start(processInfo);   // Start that process.
                while (!process.StandardOutput.EndOfStream)
                {
                    result = process.StandardOutput.ReadToEnd();
                }
                process.WaitForExit();
                if (result.Contains("LeaseExpirationTime") && result.Contains("LeaseStartTime"))
                {
                    string aLine;
                    string leaseStart = string.Empty;
                    string leaseExpiry = string.Empty;

                    StringReader strReader = new StringReader(result);
                    while (true)
                    {
                        aLine = strReader.ReadLine();
                        if (aLine != null)
                        {
                            if (aLine.Contains("LeaseExpirationTime"))
                            {
                                leaseExpiry = aLine.Replace("LeaseExpirationTime :", "").Replace('\n', ' ').Trim();
                            }
                            if (aLine.Contains("LeaseStartTime"))
                            {
                                leaseStart = aLine.Replace("LeaseStartTime :", "").Replace('\n', ' ').Trim();
                            }
                            if (!string.IsNullOrEmpty(leaseExpiry) && !string.IsNullOrEmpty(leaseStart))
                            {
                                returnval = new KeyValuePair<string, string>(leaseStart, leaseExpiry);
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return returnval;
        }
        public static List<IPAddress?> GetAllIPAddresses()
        {
            List<IPAddress?> ips = new List<IPAddress?>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (IPAddress? ip in host.AddressList.Where(ip => (ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6)))
            {
                if (!exceptionpatterns.Contains(ip.ToString()))
                {
                    string tmp = ip.ToString();
                    if (tmp.Contains('%'))
                    {
                        string[] parts = tmp.Split('%');
                        int index = int.Parse(parts[1]) - 1;
                        if (index >= ifaces.Count()) 
                        {
                            continue;
                        }
                        if (ifaces[index].OperationalStatus.Equals(OperationalStatus.Up))
                        {
                            ips.Add(ip);
                        }
                    }
                    else
                    {
                        ips.Add(ip);
                    }
                }

            }
            return ips;
        }
        public static int GetAvailablePort(int startingPort)
        {
            var portArray = new List<int>();
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);
            //getting active tcp listners - WCF service listening in tcp
            IPEndPoint[] endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);
            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);
            portArray.Sort();
            for (var i = startingPort; i < ushort.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;
            return 0;
        }
    }
}
