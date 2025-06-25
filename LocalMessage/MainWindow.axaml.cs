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
        /// UDP�ͻ���
        /// </summary>
        private readonly UdpClientWithMulticast udpclient;
        /// <summary>
        /// �ļ����շ���
        /// </summary>
        private readonly FileReceiverServer fileReceiverServer;
        /// <summary>
        /// �ļ����ͷ���
        /// </summary>
        private readonly FileSenderClient fileSenderClient;
        /// <summary>
        /// CancelToken
        /// </summary>
        private CancellationTokenSource fileserverCTS;
        /// <summary>
        /// ��ʱ��
        /// </summary>
        private System.Timers.Timer timer;
        /// <summary>
        /// ��ʱ����ʼʱ��
        /// </summary>
        private DateTime timerStartTime;
        /// <summary>
        /// �ļ����������ֵ�ԣ�����ϢID���ļ�·��
        /// </summary>
        private Dictionary<FileSendApply, string> keyValuePairsFileApply = new Dictionary<FileSendApply, string>();
        public MainWindow()
        {
            InitializeComponent();
            udpclient = new UdpClientWithMulticast();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = mainWindowViewModel;

            if (Design.IsDesignMode) return;
            //�����ļ��ͻ���
            fileSenderClient = new FileSenderClient();
            //�ļ�����
            fileserverCTS = new CancellationTokenSource();
            string filesurl = Path.Combine(AppContext.BaseDirectory, "files");
            fileReceiverServer = new FileReceiverServer(8082, filesurl);
            Task.Run(async
                () =>
            {
                await fileReceiverServer.Start();
            }, fileserverCTS.Token);
            //�������������ʾ��
            FileTipsStackPanel.IsVisible = false;


        }

        private void Button_btn_close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            //lifetime.Shutdown(); // ���Źر�
            // ��ָ���˳���
            lifetime.Shutdown(0);
        }
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (Design.IsDesignMode) return;
            //����
            udpclient.Received += (sender, e) =>
            {
                txt_rec.Text += $"\r\n------������Ϣ---����{e.OriginIp}---\r\n" + $"{e.Content}";
                var scrollViewer = txt_rec.GetTemplateChildren()
                          .OfType<ScrollViewer>()
                          .FirstOrDefault();
                scrollViewer?.ScrollToEnd();
            };
            //����
            udpclient.Joined += (sender, e) =>
            {
                label_zt.Content = "�Ѽ���";
                btn_send.IsEnabled = true;
            };
            udpclient.Exited += (sender, e) =>
            {
                label_zt.Content = "δ����";
                btn_send.IsEnabled = false;
            };
            //�����ھ�
            udpclient.Neighbourhooddiscovered += (sender, e) =>
            {
                MainWindowViewModel mwvm = (MainWindowViewModel)this.DataContext;
                if (!mwvm.Neighbourhoods.Where(o => o.Name == e.NeighbourhoodIp).Any())
                {
                    mwvm.Neighbourhoods.Add(new Neighbourhood() { Name = e.NeighbourhoodIp });
                }
            };
            //�ļ����ս�����ʾ
            fileReceiverServer.FileProgress += (sender, e) =>
            {
                btn_file_accept.IsVisible = false;
                btn_file_reject.IsVisible = false;
                if (e.state != "�������")
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
                if (e.state != "�������")
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
            //�յ��ļ���������
            udpclient.FileSendApplied += (sender, e) =>
            {
                //��ʾ������ť���в���
                btn_file_accept.IsVisible = true;
                btn_file_reject.IsVisible = true;
                FileTipsStackPanel.IsVisible = true;
                FileMsgTips.Content = $"{e.originIp}���ڸ�������{e.FileName},�Ƿ�ͬ�⣿";
                progressbar_file.Value = 0;
                keyValuePairsFileApply.Clear();
                keyValuePairsFileApply.Add(e, string.Empty);
            };
            //�յ��ļ�����������Ӧ
            udpclient.FileSendReplied += async (sender, e) =>
            {
                if (e.IsReply)
                {
                    //ͬ�ⷢ��
                    FileSendApply reply = keyValuePairsFileApply.Keys.Where(o => o.MsgID == e.RelationMsgID).FirstOrDefault();
                    if (reply == null) return;
                    await fileSenderClient.SendFile(reply.destIp, 8082, keyValuePairsFileApply[reply]);
                }
                else
                {
                    FileMsgTips.Content = $"�Է��Ѿܾ���";
                }

            };
            btn_send.IsEnabled = false;

            //Ĭ�ϼ����鲥
            udpclient.JoinMulticastGroup();
            btn_joingroup.Content = "�˳��鲥";
        }
        /// <summary>
        /// �����鲥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_joingroup_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (btn_joingroup.Content == "�����鲥")
            {
                udpclient.JoinMulticastGroup();
                btn_joingroup.Content = "�˳��鲥";
            }
            else
            {
                udpclient.LeaveMulticastGroup();
                btn_joingroup.Content = "�����鲥";
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

            txt_rec.Text += "\r\n------�ѷ�����Ϣ------\r\n" + txt_send.Text;
            //txt_rec.ScrollToLine(txt_rec.GetLineCount() - 1);
            //��ȡ TextBox �ڲ�ģ���е������ӿؼ�,����Щ�ӿؼ���ɸѡ������Ϊ ScrollViewer �Ŀؼ���ȡ��һ���ҵ��� ScrollViewer
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
        /// ���ļ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_openfolder_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Utils.OpenFolderInFileManager(Path.Combine(AppContext.BaseDirectory, "files"));
        }
        /// <summary>
        /// �����ļ�
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
                    //��ʾ����Ľ���ȷ����
                    btn_file_accept.IsVisible = false;
                    btn_file_reject.IsVisible = false;
                    FileTipsStackPanel.IsVisible = true;
                    FileMsgTips.Content = "�ȴ��Է�ȷ�ϣ�";
                    progressbar_file.Value = 0;
                    //�����ļ�����
                    MessageDataTransfeObject mdto = new MessageDataTransfeObject();
                    mdto.MsgType = "5";
                    FileSendApply fileSendApply = new FileSendApply();
                    fileSendApply.originIp = Utils.GetPrimaryIPv4Address().ToString();
                    fileSendApply.destIp = neighbour.Name;
                    fileSendApply.MsgID = Guid.NewGuid().ToString();
                    fileSendApply.FileName = Path.GetFileName(filepath);
                    mdto.Message = JsonSerializer.Serialize(fileSendApply);
                    //���������ֵ��
                    keyValuePairsFileApply.Add(fileSendApply, filepath);
                    udpclient.Send(mdto, fileSendApply.destIp);
                    //��ʱ��
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
        /// ��תURL
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
        /// �����ļ�
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
        /// �ܾ��ļ�
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
            //�ܾ�����ʾ
            btn_file_accept.IsVisible = false;
            btn_file_reject.IsVisible = false;
            FileTipsStackPanel.IsVisible = true;
            FileMsgTips.Content = $"�Ѿܾ�";
        }
    }
}