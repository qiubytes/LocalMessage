using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using LocalMessage.MsgDto;
using LocalMessage.MsgDto.impls;
using LocalMessage.Servers;
using LocalMessage.ServersClients;
using LocalMessage.ViewModel.MainWindow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMessage
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// UDP客户端
        /// </summary>
        private readonly UdpClientWithMulticast udpclient;
        /// <summary>
        /// 文件接收服务
        /// </summary>
        private readonly FileReceiverServer fileReceiverServer;
        /// <summary>
        /// 文件发送服务
        /// </summary>
        private readonly FileSenderClient fileSenderClient;
        /// <summary>
        /// CancelToken
        /// </summary>
        private CancellationTokenSource fileserverCTS;
        /// <summary>
        /// 定时器
        /// </summary>
        private System.Timers.Timer timer;
        /// <summary>
        /// 定时器开始时间
        /// </summary>
        private DateTime timerStartTime;
        /// <summary>
        /// 文件发送请求键值对，存消息ID、文件路径
        /// </summary>
        private Dictionary<FileSendApply, string> keyValuePairsFileApply = new Dictionary<FileSendApply, string>();
        public MainWindow()
        {
            InitializeComponent();
            udpclient = new UdpClientWithMulticast();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = mainWindowViewModel;

            if (Design.IsDesignMode) return;
            //发送文件客户端
            fileSenderClient = new FileSenderClient();
            //文件服务
            fileserverCTS = new CancellationTokenSource();
            string filesurl = Path.Combine(AppContext.BaseDirectory, "files");
            fileReceiverServer = new FileReceiverServer(8082, filesurl);
            Task.Run(async
                () =>
            {
                await fileReceiverServer.Start();
            }, fileserverCTS.Token);
            //隐藏最下面的提示栏
            FileTipsStackPanel.IsVisible = false;


        }

        private void Button_btn_close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            //lifetime.Shutdown(); // 优雅关闭
            // 或指定退出码
            lifetime.Shutdown(0);
        }
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (Design.IsDesignMode) return;
            //接收
            udpclient.Received += (sender, e) =>
            {
                txt_rec.Text += $"\r\n------接收消息---来自{e.OriginIp}---\r\n" + $"{e.Content}";
                var scrollViewer = txt_rec.GetTemplateChildren()
                          .OfType<ScrollViewer>()
                          .FirstOrDefault();
                scrollViewer?.ScrollToEnd();
            };
            //加入
            udpclient.Joined += (sender, e) =>
            {
                label_zt.Content = "已加入";
                btn_send.IsEnabled = true;
            };
            udpclient.Exited += (sender, e) =>
            {
                label_zt.Content = "未加入";
                btn_send.IsEnabled = false;
            };
            //发现邻居
            udpclient.Neighbourhooddiscovered += (sender, e) =>
            {
                MainWindowViewModel mwvm = (MainWindowViewModel)this.DataContext;
                if (!mwvm.Neighbourhoods.Where(o => o.Name == e.NeighbourhoodIp).Any())
                {
                    mwvm.Neighbourhoods.Add(new Neighbourhood() { Name = e.NeighbourhoodIp });
                }
            };
            //文件接收进度显示
            fileReceiverServer.FileProgress += (sender, e) =>
            {
                btn_file_accept.IsVisible = false;
                btn_file_reject.IsVisible = false;
                if (e.state != "传输完成")
                {
                    FileTipsStackPanel.IsVisible = true;
                }
                //else
                //{
                //    FileTipsStackPanel.IsVisible = false;
                //}
                int current = Convert.ToInt32(Convert.ToDecimal(e.currentBytes) / e.totalBytes * 100);
                FileMsgTips.Content = e.msg + e.state + current.ToString() + "%";
                progressbar_file.Value = current;
            };
            fileSenderClient.SendProgress += (sender, e) =>
            {
                btn_file_accept.IsVisible = false;
                btn_file_reject.IsVisible = false;
                if (e.state != "传输完成")
                {
                    FileTipsStackPanel.IsVisible = true;
                }
                //else
                //{
                //    FileTipsStackPanel.IsVisible = false;
                //}
                int current = Convert.ToInt32(Convert.ToDecimal(e.currentBytes) / e.totalBytes * 100);
                FileMsgTips.Content = e.msg + e.state + current.ToString() + "%";
                progressbar_file.Value = current;
            };
            //收到文件发送请求
            udpclient.FileSendApplied += (sender, e) =>
            {
                //显示操作按钮进行操作
                btn_file_accept.IsVisible = true;
                btn_file_reject.IsVisible = true;
                FileTipsStackPanel.IsVisible = true;
                FileMsgTips.Content = $"{e.originIp}正在给您发送{e.FileName},是否同意？";
                progressbar_file.Value = 0;
                keyValuePairsFileApply.Clear();
                keyValuePairsFileApply.Add(e, string.Empty);
            };
            //收到文件发送请求响应
            udpclient.FileSendReplied += async (sender, e) =>
            {
                if (e.IsReply)
                {
                    //同意发送
                    FileSendApply reply = keyValuePairsFileApply.Keys.Where(o => o.MsgID == e.RelationMsgID).FirstOrDefault();
                    if (reply == null) return;
                    await fileSenderClient.SendFile(reply.destIp, 8082, keyValuePairsFileApply[reply]);
                }
                else
                {
                    FileMsgTips.Content = $"对方已拒绝！";
                }

            };
            btn_send.IsEnabled = false;

            //默认加入组播
            udpclient.JoinMulticastGroup();
            btn_joingroup.Content = "退出组播";
        }
        /// <summary>
        /// 加入组播
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_joingroup_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (btn_joingroup.Content == "加入组播")
            {
                udpclient.JoinMulticastGroup();
                btn_joingroup.Content = "退出组播";
            }
            else
            {
                udpclient.LeaveMulticastGroup();
                btn_joingroup.Content = "加入组播";
            }
        }

        private void btn_send_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MultiCastMsg multiCastMsg = new MultiCastMsg()
            {
                originIp = Utils.GetPrimaryIPv4Address().ToString(),
                content = txt_send.Text
            };
            MessageDataTransfeObject mdtso = new MessageDataTransfeObject()
            {
                MsgType = "1",
                Message = JsonSerializer.Serialize(multiCastMsg)
            };
            udpclient.SendMulticastMessage(mdtso);

            txt_rec.Text += "\r\n------已发送消息------\r\n" + txt_send.Text;
            //txt_rec.ScrollToLine(txt_rec.GetLineCount() - 1);
            //获取 TextBox 内部模板中的所有子控件,从这些子控件中筛选出类型为 ScrollViewer 的控件，取第一个找到的 ScrollViewer
            var scrollViewer = txt_rec.GetTemplateChildren()
                          .OfType<ScrollViewer>()
                          .FirstOrDefault();
            scrollViewer?.ScrollToEnd();
        }

        private void btn_scan_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainWindowViewModel mwvm = (MainWindowViewModel)this.DataContext;
            mwvm?.Neighbourhoods.Clear();
            MessageDataTransfeObject mdtso = new MessageDataTransfeObject()
            {
                MsgType = "2",
                Message = Utils.GetPrimaryIPv4Address().ToString()
            };
            udpclient.SendMulticastMessage(mdtso);
        }
        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_openfolder_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Utils.OpenFolderInFileManager(Path.Combine(AppContext.BaseDirectory, "files"));
        }
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_sendfile_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string? filepath = await Utils.SelectSingleFile(this);
            if (!string.IsNullOrEmpty(filepath))
            {

                Neighbourhood neighbour = (Neighbourhood)NeighbourHoodList.SelectedItem;
                if (neighbour != null)
                {
                    // await fileSenderClient.SendFile(neighbour.Name, 8082, filepath);
                    //显示下面的进度确认栏
                    btn_file_accept.IsVisible = false;
                    btn_file_reject.IsVisible = false;
                    FileTipsStackPanel.IsVisible = true;
                    FileMsgTips.Content = "等待对方确认！";
                    progressbar_file.Value = 0;
                    //发送文件请求
                    MessageDataTransfeObject mdto = new MessageDataTransfeObject();
                    mdto.MsgType = "5";
                    FileSendApply fileSendApply = new FileSendApply();
                    fileSendApply.originIp = Utils.GetPrimaryIPv4Address().ToString();
                    fileSendApply.destIp = neighbour.Name;
                    fileSendApply.MsgID = Guid.NewGuid().ToString();
                    fileSendApply.FileName = Path.GetFileName(filepath);
                    mdto.Message = JsonSerializer.Serialize(fileSendApply);
                    //存入请求键值对
                    keyValuePairsFileApply.Add(fileSendApply, filepath);
                    udpclient.Send(mdto, fileSendApply.destIp);
                    //定时器
                    //timer = new System.Timers.Timer();
                    //timer.Interval = 30 * 1000;
                    //timer.Elapsed += (sender, e) =>
                    //{
                    //    timer.Stop();
                    //    btn_file_accept.IsVisible = false;
                    //    btn_file_reject.IsVisible = false;
                    //    FileTipsStackPanel.IsVisible = false;
                    //    keyValuePairsFileApply.Clear();
                    //};
                }
            }
        }


        /// <summary>
        /// 跳转URL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void githuburl_tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/qiubytes/LocalMessage",
                UseShellExecute = true
            });
        }
        /// <summary>
        /// 接受文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_file_accept_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MessageDataTransfeObject mdto = new MessageDataTransfeObject();
            mdto.MsgType = "6";
            FileSendReply fileSendReply = new FileSendReply();
            fileSendReply.RelationMsgID = keyValuePairsFileApply.FirstOrDefault().Key.MsgID;
            fileSendReply.IsReply = true;
            mdto.Message = JsonSerializer.Serialize(fileSendReply);
            udpclient.Send(mdto, keyValuePairsFileApply.FirstOrDefault().Key.originIp);
        }
        /// <summary>
        /// 拒绝文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_file_reject_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MessageDataTransfeObject mdto = new MessageDataTransfeObject();
            mdto.MsgType = "6";
            FileSendReply fileSendReply = new FileSendReply();
            fileSendReply.RelationMsgID = keyValuePairsFileApply.FirstOrDefault().Key.MsgID;
            fileSendReply.IsReply = false;
            mdto.Message = JsonSerializer.Serialize(fileSendReply);
            udpclient.Send(mdto, keyValuePairsFileApply.FirstOrDefault().Key.originIp);
            //拒绝后提示
            btn_file_accept.IsVisible = false;
            btn_file_reject.IsVisible = false;
            FileTipsStackPanel.IsVisible = true;
            FileMsgTips.Content = $"已拒绝";
        }
    }
}