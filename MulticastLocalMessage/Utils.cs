using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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
        /// <summary>
        /// 打开指定文件夹
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void OpenFolderInFileManager(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"文件夹不存在: {folderPath}");
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start("explorer.exe", $"\"{folderPath}\"");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", $"\"{folderPath}\"");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // 尝试多个可能的文件管理器
                    try
                    {
                        Process.Start("xdg-open", $"\"{folderPath}\"");
                    }
                    catch
                    {
                        try { Process.Start("nautilus", $"\"{folderPath}\""); } catch { }
                        try { Process.Start("dolphin", $"\"{folderPath}\""); } catch { }
                        try { Process.Start("thunar", $"\"{folderPath}\""); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理异常，如显示错误消息
                Console.WriteLine($"无法打开文件夹: {ex.Message}");
            }
        }
    }
}
