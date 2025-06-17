using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using MulticastLocalMessage.Events;

namespace MulticastLocalMessage
{
    public class MulticastHelper
    {
        private readonly string MulticastAddress = "239.255.255.250"; // 组播地址
        private readonly int MulticastPort = 5000; // 组播端口
        private bool isRunning = true;
        private UdpClient udpClient;
        /// <summary>
        /// 接收消息
        /// </summary>
        public EventHandler<ReceiveMsg>? Received;
        /// <summary>
        /// 加入组播
        /// </summary>
        public EventHandler<EventArgs>? Joined;
        /// <summary>
        /// 退出组播
        /// </summary>
        public EventHandler<EventArgs>? Exited;

        public MulticastHelper()
        {
            udpClient = new UdpClient();
        }
        /// <summary>
        /// 加入组播组
        /// </summary>
        public void JoinMulticastGroup()
        {
            try
            {
                if (udpClient != null)
                {
                    udpClient.Close();
                }

                // 创建UDP客户端并绑定到任意可用端口
                udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);//允许多个套接字绑定相同端口
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastPort));
                // 加入组播组
                udpClient.JoinMulticastGroup(IPAddress.Parse(MulticastAddress));
                Console.WriteLine($"已加入组播组 {MulticastAddress}:{MulticastPort}");

                Joined?.Invoke(this, EventArgs.Empty);
                StartThread();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加入组播组时出错: {ex.Message}");
            }
        }
        /// <summary>
        /// 开启接收线程
        /// </summary>
        public void StartThread()
        {
            // 启动接收线程
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMulticastMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                IPEndPoint multicastEndpoint = new IPEndPoint(IPAddress.Parse(MulticastAddress), MulticastPort);
                udpClient.Send(data, data.Length, multicastEndpoint);
                Console.WriteLine($"已发送消息: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息时出错: {ex.Message}");
            }
        }
        /// <summary>
        /// 接收消息
        /// </summary>
        public void ReceiveMessages()
        {
            try
            {
                while (isRunning)
                {
                    IPEndPoint remoteEndpoint = null;
                    byte[] receivedData = udpClient.Receive(ref remoteEndpoint);
                    string receivedMessage = Encoding.UTF8.GetString(receivedData);
                    Console.WriteLine($"收到来自 {remoteEndpoint} 的消息: {receivedMessage}");
                    //接收事件
                    Dispatcher.UIThread.Post(() =>
                    {
                        Received?.Invoke(this, new ReceiveMsg() { OriginIp = remoteEndpoint.ToString(), Content = receivedMessage });
                    });
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                // 正常退出
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收消息时出错: {ex.Message}");
            }
        }
        /// <summary>
        /// 退出组播组
        /// </summary>
        public void LeaveMulticastGroup()
        {
            try
            {
                if (udpClient != null)
                {
                    udpClient.DropMulticastGroup(IPAddress.Parse(MulticastAddress));
                    udpClient.Close();
                    udpClient = null;
                    Console.WriteLine($"已退出组播组 {MulticastAddress}:{MulticastPort}");
                    Exited?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"退出组播组时出错: {ex.Message}");
            }
        }
    }
}
