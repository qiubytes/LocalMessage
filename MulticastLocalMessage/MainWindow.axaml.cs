using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using MulticastLocalMessage.MsgDto;
using MulticastLocalMessage.MsgDto.impls;
using MulticastLocalMessage.Servers;
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
        private readonly MulticastHelper _multicastHelper;
        private readonly FileReceiverServer fileReceiverServer;
        private CancellationTokenSource fileserverCTS;
        public MainWindow()
        {
            InitializeComponent();
            _multicastHelper = new MulticastHelper();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            mainWindowViewModel.Neighbourhoods = new System.Collections.ObjectModel.ObservableCollection<Neighbourhood>();
            mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.2" });
            mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.3" });
            mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.4" });
            mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.8" });
            mainWindowViewModel.Neighbourhoods.Add(new Neighbourhood() { Name = "192.168.1.10" });
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
            _multicastHelper.Received += (sender, e) =>
            {
                txt_rec.Text += $"\r\n------������Ϣ---����{e.OriginIp}---\r\n" + $"{e.Content}";
                var scrollViewer = txt_rec.GetTemplateChildren()
                          .OfType<ScrollViewer>()
                          .FirstOrDefault();
                scrollViewer?.ScrollToEnd();
            };
            //����
            _multicastHelper.Joined += (sender, e) =>
            {
                label_zt.Content = "�Ѽ���";
                btn_send.IsEnabled = true;
            };
            _multicastHelper.Exited += (sender, e) =>
            {
                label_zt.Content = "δ����";
                btn_send.IsEnabled = false;
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
                _multicastHelper.JoinMulticastGroup();
                btn_joingroup.Content = "�˳��鲥";
            }
            else
            {
                _multicastHelper.LeaveMulticastGroup();
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
            _multicastHelper.SendMulticastMessage(mdtso);

            txt_rec.Text += "\r\n------�ѷ�����Ϣ------\r\n" + txt_send.Text;
            //txt_rec.ScrollToLine(txt_rec.GetLineCount() - 1);
            //��ȡ TextBox �ڲ�ģ���е������ӿؼ�,����Щ�ӿؼ���ɸѡ������Ϊ ScrollViewer �Ŀؼ���ȡ��һ���ҵ��� ScrollViewer
            var scrollViewer = txt_rec.GetTemplateChildren()
                          .OfType<ScrollViewer>()
                          .FirstOrDefault();
            scrollViewer?.ScrollToEnd();
        }
    }
}