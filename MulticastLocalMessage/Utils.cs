using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage
{
    public class Utils
    {
        /// <summary>
        /// 获取主活动网卡的IPv4地址
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetPrimaryIPv4Address()
        {
            var activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                    !ni.Description.Contains("Virtual") &&
                                    !ni.Description.Contains("Pseudo") &&
                                    !ni.Name.StartsWith("vEthernet"));

            if (activeInterface != null)
            {
                var ip = activeInterface.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                return ip?.Address;
            }

            return null;
        }
    }
}
