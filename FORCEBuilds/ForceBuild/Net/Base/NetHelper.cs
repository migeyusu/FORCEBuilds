using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace FORCEBuild.Net.Base
{
    public class NetHelper
    {
        public static IEnumerable<IPAddress> Ipv4Collection => Dns.GetHostAddresses(Dns.GetHostName()).Where(ipa => ipa.AddressFamily == AddressFamily.InterNetwork);

        public static IEnumerable<string> Ipv4StringCollection => Dns.GetHostAddresses(Dns.GetHostName()).Where(ipa => ipa.AddressFamily == AddressFamily.InterNetwork)
            .Select(ipAddress => ipAddress.ToString());

        public static IPAddress InstanceIpv4
        {
            get
            {
                var hostname = Dns.GetHostName();
                var ipas = Dns.GetHostAddresses(hostname);
                return ipas.FirstOrDefault(ipa => ipa.AddressFamily == AddressFamily.InterNetwork);
            }       
        }

        /// <summary>
        /// 即请求即用，防止相同终结点被重复请求
        /// </summary>
        public static int AvailablePort
        {
            get
            {
                var ipgp = IPGlobalProperties.GetIPGlobalProperties();
               
                var tcpip = ipgp.GetActiveTcpListeners();
                var udpip = ipgp.GetActiveUdpListeners();
                var tc = ipgp.GetActiveTcpConnections();
                var al = tcpip.ToList();
                al.AddRange(udpip);
                al.AddRange(tc.Select(ti => ti.LocalEndPoint));
                for (var i = 1000; i < 65535; i++)
                {
                    var end = true;
                    foreach (var ep in al)
                    {
                        if (ep.Port != i) continue;
                        al.Remove(ep);
                        end = false;
                        break;
                    }
                    if (end)
                        return i;
                }
                return -1;
            }
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///         <paramref name="port" /> is less than <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.-or- <paramref name="address" /> is less than 0 or greater than 0x00000000FFFFFFFF. </exception>
        public static IPEndPoint InstanceEndPoint => new IPEndPoint(InstanceIpv4, AvailablePort);
    }
}
