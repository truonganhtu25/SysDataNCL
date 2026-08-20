using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using iPhoneSystemCleaner.Views;
using iPhoneSystemCleaner.Services;

namespace iPhoneSystemCleaner
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public SshService Ssh { get; } = new SshService();

        public MainWindow()
        {
            try
            {
                Instance = this;
                InitializeComponent();

                // Wire up child views
                ViewConnect.OnConnected += OnDeviceConnected;
                ViewScan.OnScanComplete += OnScanComplete;
                ViewResults.OnDeleteComplete += OnDeleteComplete;

                // Pass SSH service to views
                ViewConnect.Initialize(Ssh);
                ViewScan.Initialize(Ssh);
                ViewResults.Initialize(Ssh);
                ViewRestore.Initialize(Ssh);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException;
                string msg = inner != null ? inner.Message + "\n" + inner.StackTrace : ex.Message + "\n" + ex.StackTrace;
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                System.IO.File.WriteAllText(System.IO.Path.Combine(desktop, "crash_mainwindow.txt"), msg);
                MessageBox.Show($"MainWindow Init Error: {msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void ShowView(UIElement toShow, Button activeTab)
        {
            ViewConnect.Visibility = Visibility.Collapsed;
            ViewScan.Visibility = Visibility.Collapsed;
            ViewResults.Visibility = Visibility.Collapsed;
            ViewRestore.Visibility = Visibility.Collapsed;

            toShow.Visibility = Visibility.Visible;

            BtnTabConnect.Style = FindResource("TabBtn") as Style;
            BtnTabScan.Style = FindResource("TabBtn") as Style;
            BtnTabResults.Style = FindResource("TabBtn") as Style;
            BtnTabRestore.Style = FindResource("TabBtn") as Style;
            activeTab.Style = FindResource("TabBtnActive") as Style;

            // Fade and slide animation
            var transform = new TranslateTransform(0, 15);
            toShow.RenderTransform = transform;
            
            var opacityAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(250)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            var slideAnim = new DoubleAnimation(15, 0, new Duration(TimeSpan.FromMilliseconds(250)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            toShow.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            transform.BeginAnimation(TranslateTransform.YProperty, slideAnim);
        }

        private void BtnTabConnect_Click(object sender, RoutedEventArgs e)
            => ShowView(ViewConnect, BtnTabConnect);

        private void BtnTabScan_Click(object sender, RoutedEventArgs e)
            => ShowView(ViewScan, BtnTabScan);

        private void BtnTabResults_Click(object sender, RoutedEventArgs e)
            => ShowView(ViewResults, BtnTabResults);

        private void BtnTabRestore_Click(object sender, RoutedEventArgs e)
            => ShowView(ViewRestore, BtnTabRestore);

        // ══ Device connected callback ══
        private void OnDeviceConnected(DeviceInfo info)
        {
            Dispatcher.Invoke(() =>
            {
                DeviceInfoPanel.Visibility = Visibility.Visible;
                TbDeviceName.Text = info.Hostname;
                TbDeviceIos.Text = $" · iOS {info.IosVersion}";
                TbStatus.Text = $"✅ Đã kết nối — {info.Hostname} · {info.JailbreakType} · Dung lượng: {info.DiskUsedDisplay}/{info.DiskTotalDisplay}";

                BtnTabScan.IsEnabled = true;

                // Auto-navigate to scan
                ShowView(ViewScan, BtnTabScan);
            });
        }

        // ══ Scan complete callback ══
        private void OnScanComplete()
        {
            Dispatcher.Invoke(() =>
            {
                BtnTabResults.IsEnabled = true;
                ShowView(ViewResults, BtnTabResults);
                TbStatus.Text = "✅ Quét xong — Chọn mục cần xóa và nhấn 'Xóa đã chọn'";
            });
        }

        // ══ Delete complete callback ══
        private void OnDeleteComplete(long freedBytes)
        {
            Dispatcher.Invoke(() =>
            {
                TbStatus.Text = $"🎉 Đã xóa xong — Giải phóng được {FormatSize(freedBytes)}";
            });
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / 1024.0 / 1024.0:F1} MB";
            return $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
        }

        // ══ Window chrome ══
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ToggleMaximize();
            else
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void BtnMaxRestore_Click(object sender, RoutedEventArgs e)
            => ToggleMaximize();

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                BtnMaxRestore.Content = "\xE922"; // Maximize icon
            }
            else
            {
                WindowState = WindowState.Maximized;
                BtnMaxRestore.Content = "\xE923"; // Restore down icon
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Ssh.Dispose();
            Close();
        }
    }
}