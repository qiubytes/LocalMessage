using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

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
            //lifetime.Shutdown(); // 优雅关闭
            // 或指定退出码
            lifetime.Shutdown(0);
        }
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            //接收
            _multicastHelper.Received += (sender, e) =>
            {
                txt_rec.Text = e.Content;
            };
            //加入
            _multicastHelper.Joined += (sender, e) =>
            {
                label_zt.Content = "已加入";
                btn_send.IsEnabled = true; 
            };
            _multicastHelper.Exited += (sender, e) =>
            {
                label_zt.Content = "未加入";
                btn_send.IsEnabled = false;
            };

            btn_send.IsEnabled = false;
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
                _multicastHelper.JoinMulticastGroup();
                btn_joingroup.Content = "退出组播";
            }
            else
            {
                _multicastHelper.LeaveMulticastGroup();
                btn_joingroup.Content = "加入组播";
            }
        }

        private void btn_send_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _multicastHelper.SendMulticastMessage(txt_send.Text);
        }
    }
}