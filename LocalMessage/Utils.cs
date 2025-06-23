using Avalonia.Controls;
using Avalonia.Platform.Storage;
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

namespace LocalMessage
{
    /// <summary>
    /// 辅助工具类
    /// </summary>
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
        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static async Task<string?> SelectSingleFile(Window window)
        {
            // 获取 TopLevel 引用 (通常是当前窗口)
            var topLevel = TopLevel.GetTopLevel(window);

            if (topLevel != null)
            {
                // 配置文件选择选项
                var options = new FilePickerOpenOptions
                {
                    Title = "选择文件",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {

                        new FilePickerFileType("所有文件")
                        {
                            Patterns = new[] { "*" }
                        }
                     }
                };

                // 打开文件选择对话框
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

                // 返回第一个选择的文件路径
                return files.FirstOrDefault()?.Path.LocalPath;
            }

            return null;
        }
    }
}
