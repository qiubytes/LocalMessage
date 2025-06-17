using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using System.Linq;

namespace MulticastLocalMessage
{
    public partial class MainWindow : Window
    {
        private readonly MulticastHelper _multicastHelper;
        public MainWindow()
        {
            InitializeComponent();
            _multicastHelper = new MulticastHelper();
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
            _multicastHelper.SendMulticastMessage(txt_send.Text);

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