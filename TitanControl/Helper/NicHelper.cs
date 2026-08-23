using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TitanControl.Logging;

namespace TitanControl.Helper
{
    public class NicHelper
    {
        public static Dictionary<string, IPAddress> GetNics()
        {
            var dic = new Dictionary<string, IPAddress>();

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var address = nic.GetIPProperties()
                    .UnicastAddresses
                    .FirstOrDefault(a =>
                        a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;

                if (address == null)
                {
                    Log.Warning($"No IPv4 address found for {nic.Name}", "NicHelper");
                    continue;
                }

                dic.Add(nic.Name, address);
            }

            return dic;
        }

        public static IPAddress GetDefaultIPv4Address()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect("8.8.8.8", 53);

            return ((IPEndPoint)socket.LocalEndPoint!).Address;
        }
    }
}
