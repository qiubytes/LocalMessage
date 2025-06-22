using Avalonia.Threading;
using MulticastLocalMessage.Events;
using MulticastLocalMessage.MsgDto;
using MulticastLocalMessage.MsgDto.impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MulticastLocalMessage.ServersClients
{
    /// <summary>
    /// UdpClient (包括单播、组播)
    /// </summary>
    public class UdpClientWithMulticast
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
        /// <summary>
        /// 发现邻居
        /// </summary>
        public EventHandler<NeighbourhoodDiscovered>? Neighbourhooddiscovered;

        public UdpClientWithMulticast()
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
                /// udpClient.Ttl = 32;//设置TTL 
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);//允许多个套接字绑定相同端口
                udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 32);  // 设置TTL(udpclient设置不生效)

                //写固定IP为了指定网卡
                udpClient.Client.Bind(new IPEndPoint(Utils.GetPrimaryIPv4Address(), MulticastPort));

                //udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastPort));
                // 加入组播组
                // udpClient.MulticastLoopback = false;
                udpClient.JoinMulticastGroup(IPAddress.Parse(MulticastAddress), localAddress: Utils.GetPrimaryIPv4Address());//指定IP（网卡），组播订阅可能绑定到错误的网卡
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
        public void SendMulticastMessage(MessageDataTransfeObject msgobj)
        {
            try
            {
                string message = JsonSerializer.Serialize(msgobj);
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
                    MessageDataTransfeObject mdtso = JsonSerializer.Deserialize<MessageDataTransfeObject>(receivedMessage);
                    Console.WriteLine($"收到来自 {remoteEndpoint} 的消息: {mdtso.Message}");
                    if (mdtso.MsgType == "1")
                    {
                        MultiCastMsg multiCastMsg = JsonSerializer.Deserialize<MultiCastMsg>(mdtso.Message);
                        //接收事件
                        Dispatcher.UIThread.Post(() =>
                        {
                            Received?.Invoke(this, new ReceiveMsg() { OriginIp = multiCastMsg.originIp, Content = multiCastMsg.content });
                        });
                    }
                    else if (mdtso.MsgType == "2") //收到发现邻居消息后，自动单播发送 回应消息
                    {
                        MessageDataTransfeObject mdtsoreply = new MessageDataTransfeObject()
                        {
                            MsgType = "3",
                            Message = Utils.GetPrimaryIPv4Address().ToString()
                        };
                        string message = JsonSerializer.Serialize(mdtsoreply);
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(mdtso.Message), MulticastPort);//单播源地址发送
                        udpClient.Send(data, data.Length, endpoint);
                    }
                    else if (mdtso.MsgType == "3") //发现邻居回应
                    {
                        //发现邻居
                        Dispatcher.UIThread.Post(() =>
                        {
                            Neighbourhooddiscovered?.Invoke(this, new NeighbourhoodDiscovered() { NeighbourhoodIp = mdtso.Message });
                        });
                    }

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
