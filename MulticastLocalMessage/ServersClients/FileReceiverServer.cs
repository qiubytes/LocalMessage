using Avalonia.Threading;
using MulticastLocalMessage.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLocalMessage.Servers
{
    public class FileReceiverServer
    {
        public const int BufferSize = 8192; // 8KB缓冲区
        public const int HeaderSize = 256;  // 头部固定256字节，包含文件名和文件长度

        private readonly int _port;
        private readonly string _saveDirectory;
        public EventHandler<FileSendReceiveProgress> FileProgress;

        public FileReceiverServer(int port, string saveDirectory)
        {
            _port = port;
            _saveDirectory = saveDirectory;

            // 确保保存目录存在
            Directory.CreateDirectory(saveDirectory);
        }

        public async Task Start()
        {
            var listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();

            Console.WriteLine($"Server started on port {_port}. Waiting for connections...");

            while (true)
            {
                try
                {
                    using (var client = await listener.AcceptTcpClientAsync())
                    using (var stream = client.GetStream())
                    {
                        Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

                        // 接收文件头部信息（文件名和长度）
                        var headerBuffer = new byte[HeaderSize];
                        var bytesRead = stream.Read(headerBuffer, 0, HeaderSize);

                        if (bytesRead < HeaderSize)
                        {
                            Console.WriteLine("消息头格式不正确");
                            continue;
                        }

                        //解析头部信息
                        // 1. 找到分隔符'|'的位置
                        int separatorIndex = Array.IndexOf(headerBuffer, (byte)'|');

                        if (separatorIndex == -1)
                        {
                            throw new FormatException("未能找到分隔符 '|'");
                        }

                        // 2. 提取文件名部分
                        string fileName = Encoding.UTF8.GetString(headerBuffer, 0, separatorIndex);

                        // 3. 提取文件长度部分
                        // 找到文件长度部分的结束位置（第一个'\0'）
                        int lengthStart = separatorIndex + 1;
                        int lengthEnd = lengthStart;

                        // 遍历直到遇到第一个'\0'或到达数组末尾
                        while (lengthEnd < headerBuffer.Length && headerBuffer[lengthEnd] != 0)
                        {
                            lengthEnd++;
                        }

                        string lengthStr = Encoding.UTF8.GetString(headerBuffer, lengthStart, lengthEnd - lengthStart);

                        if (!long.TryParse(lengthStr, out long fileLength))
                        {
                            throw new FormatException("长度格式错误");
                        }

                        Console.WriteLine($"接收文件: {fileName} ({fileLength} 字节)");

                        // 接收文件内容
                        var savePath = Path.Combine(_saveDirectory, fileName);
                        using (var fileStream = File.Create(savePath))
                        {
                            var totalBytesRead = 0L;
                            var buffer = new byte[BufferSize];

                            while (totalBytesRead < fileLength)
                            {
                                bytesRead = await stream.ReadAsync(buffer, 0, (int)Math.Min(BufferSize, fileLength - totalBytesRead));
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                // 显示进度
                                Dispatcher.UIThread.Post(() =>
                                {
                                    FileProgress?.Invoke(this, new FileSendReceiveProgress()
                                    {
                                        currentBytes = totalBytesRead,
                                        totalBytes = fileLength,
                                        state = "传输中",
                                        msg = $"来自{client.Client.RemoteEndPoint.ToString()}的文件:{fileName}"
                                    });
                                });
                                // Console.Write($"\rProgress: {totalBytesRead * 100 / fileLength}%");
                            }
                            //传输完成
                            Dispatcher.UIThread.Post(() =>
                            {
                                FileProgress?.Invoke(this, new FileSendReceiveProgress()
                                {
                                    currentBytes = totalBytesRead,
                                    totalBytes = fileLength,
                                    state = "传输完成",
                                    msg = $"来自{client.Client.RemoteEndPoint.ToString()}的文件:{fileName}"
                                });
                            });
                            //Console.WriteLine("\nFile received successfully.");
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
}
