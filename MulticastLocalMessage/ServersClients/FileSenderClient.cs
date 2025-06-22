using MulticastLocalMessage.Servers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.ServersClients
{
    /// <summary>
    /// 文件发送客户端
    /// </summary>
    public class FileSenderClient
    {
        public async Task SendFile(string serverIp, int port, string filePath)
        {
            try
            {
                using (var client = new TcpClient(serverIp, port))
                using (var stream = client.GetStream())
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileInfo = new FileInfo(filePath);
                    var fileLength = fileInfo.Length;

                    //构造头部
                    var fileNameBytes = Encoding.UTF8.GetBytes(fileName);//文件名字节
                    var fileLengthStr = fileLength.ToString();//文件长度字符
                    var fileLengthBytes = Encoding.UTF8.GetBytes(fileLengthStr);//文件长度字节
                    //剩余填充字节
                    int totalHeaderSize = 256;
                    int remainingBytes = totalHeaderSize - (fileNameBytes.Length + 1 + fileLengthBytes.Length);
                    // 3. 构造字节数组
                    var headerBytes = new byte[totalHeaderSize];
                    Buffer.BlockCopy(fileNameBytes, 0, headerBytes, 0, fileNameBytes.Length);
                    headerBytes[fileNameBytes.Length] = (byte)'|'; // 分隔符
                    Buffer.BlockCopy(fileLengthBytes, 0, headerBytes, fileNameBytes.Length + 1, fileLengthBytes.Length);
                    // 剩余部分填充 \0
                    for (int i = fileNameBytes.Length + 1 + fileLengthBytes.Length; i < totalHeaderSize; i++)
                    {
                        headerBytes[i] = 0;
                    }
                    // 发送头部
                    await stream.WriteAsync(headerBytes, 0, headerBytes.Length);

                    // 发送文件内容
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        var buffer = new byte[FileReceiverServer.BufferSize];
                        int bytesRead;
                        var totalBytesSent = 0L;

                        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await stream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesSent += bytesRead;

                            // 显示进度
                            Console.Write($"进度: {totalBytesSent * 100 / fileLength}%");
                        }

                        Console.WriteLine("发送文件成功");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
