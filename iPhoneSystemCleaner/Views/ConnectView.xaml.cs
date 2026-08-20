using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iPhoneSystemCleaner.Services;

namespace iPhoneSystemCleaner.Views
{
    public partial class ConnectView : UserControl
    {
        private SshService? _ssh;
        public event Action<DeviceInfo>? OnConnected;

        public ConnectView()
        {
            InitializeComponent();
        }

        public void Initialize(SshService ssh)
        {
            _ssh = ssh;
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_ssh == null) return;

            var ip = TbIp.Text.Trim();
            var username = TbUsername.Text.Trim();
            var password = PbPassword.Password;

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(username))
            {
                ShowStatus(false, "Vui lòng nhập đầy đủ IP và tên đăng nhập");
                return;
            }

            if (!int.TryParse(TbPort.Text.Trim(), out int port))
                port = 22;

            // UI loading state
            BtnConnect.IsEnabled = false;
            BtnConnectText.Text = "Đang kết nối...";
            LoadingPanel.Visibility = Visibility.Visible;
            StatusPanel.Visibility = Visibility.Collapsed;
            DeviceInfoCard.Visibility = Visibility.Collapsed;

            var (success, message) = await _ssh.ConnectAsync(ip, username, password, port);

            LoadingPanel.Visibility = Visibility.Collapsed;
            BtnConnect.IsEnabled = true;
            BtnConnectText.Text = "Kết nối SSH";

            if (!success)
            {
                ShowStatus(false, message);
                return;
            }

            ShowStatus(true, "Kết nối thành công! Đang lấy thông tin thiết bị...");

            // Get device info
            LoadingPanel.Visibility = Visibility.Visible;
            var info = await _ssh.GetDeviceInfoAsync();
            LoadingPanel.Visibility = Visibility.Collapsed;

            // Update device info card
            TbDvHostname.Text = info.Hostname;
            TbDvIos.Text = $"iOS {info.IosVersion}";
            TbDvJbType.Text = info.JailbreakType;
            TbDvFree.Text = info.DiskFreeDisplay;
            TbDvDiskDetail.Text = $"{info.DiskUsedDisplay} / {info.DiskTotalDisplay}";
            PbDisk.Value = info.UsedPercent;

            // Change disk bar color if nearly full
            if (info.UsedPercent > 85)
                PbDisk.Foreground = FindResource("DangerBrush") as Brush;
            else if (info.UsedPercent > 70)
                PbDisk.Foreground = FindResource("WarningBrush") as Brush;

            DeviceInfoCard.Visibility = Visibility.Visible;

            // Fire callback to MainWindow
            OnConnected?.Invoke(info);
        }

        private void BtnGoScan_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to scan tab via MainWindow
            MainWindow.Instance?.BtnTabScan.RaiseEvent(
                new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        private void BtnPasteIp_Click(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText()?.Trim();
            if (!string.IsNullOrEmpty(text))
                TbIp.Text = text;
        }

        private void ShowStatus(bool success, string msg)
        {
            StatusPanel.Visibility = Visibility.Visible;
            TbStatusMsg.Text = msg;

            if (success)
            {
                TbStatusIcon.Text = "✅";
                StatusPanel.Background = new SolidColorBrush(Color.FromRgb(0x0A, 0x2A, 0x1A));
                StatusPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E));
                StatusPanel.BorderThickness = new Thickness(1);
                TbStatusMsg.Foreground = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E));
            }
            else
            {
                TbStatusIcon.Text = "❌";
                StatusPanel.Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x0A, 0x0A));
                StatusPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                StatusPanel.BorderThickness = new Thickness(1);
                TbStatusMsg.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            }
        }
    }
}
