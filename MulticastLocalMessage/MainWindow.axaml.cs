using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using MulticastLocalMessage.MsgDto;
using MulticastLocalMessage.MsgDto.impls;
using MulticastLocalMessage.Servers;
using MulticastLocalMessage.ServersClients;
using MulticastLocalMessage.ViewModel.MainWindow;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MulticastLocalMessage
{
    public partial class MainWindow : Window
    {
        private readonly UdpClientWithMulticast udpclient;
        private readonly FileReceiverServer fileReceiverServer;
        private CancellationTokenSource fileserverCTS;
        public MainWindow()
        {
            InitializeComponent();
            udpclient = new UdpClientWithMulticast();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            //mainWindowViewModel.Neighbourhoods = new System.Collections.ObjectModel.ObservableCollection<Neighbourhood>();
            //mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.2" });
            //mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.3" });
            //mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.4" });
            //mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.8" });
            //mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.10" });
            this.DataContext = mainWindowViewModel;

            if (Design.IsDesignMode) return;
            //�ļ�����
            fileserverCTS = new CancellationTokenSource();
            string filesurl = Path.Combine(AppContext.BaseDirectory, "files");
            fileReceiverServer = new FileReceiverServer(8082, filesurl);
            Task.Run(async
                () =>
            {
                await fileReceiverServer.Start();
            }, fileserverCTS.Token);
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
            btn_send.IsEnabled = false;
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
            MessageDataTransfeObject mdtso = new MessageDataTransfeObject()
            {
                MsgType = "2",
                Message = Utils.GetPrimaryIPv4Address().ToString()
            };
            udpclient.SendMulticastMessage(mdtso);
        }

        private void btn_openfolder_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Utils.OpenFolderInFileManager(Path.Combine(AppContext.BaseDirectory, "files"));
        }

        private async void btn_sendfile_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string? filepath = await Utils.SelectSingleFile(this);
            if (!string.IsNullOrEmpty(filepath))
            {
                FileSenderClient client = new FileSenderClient();
                Neighbourhood neighbour = (Neighbourhood)NeighbourHoodList.SelectedItem;
                if (neighbour != null)
                {
                    await client.SendFile(neighbour.Name, 8082, filepath); 
                }
            }
        }
    }
}