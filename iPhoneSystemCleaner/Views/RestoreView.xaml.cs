using System;
using System.Windows;
using System.Windows.Controls;
using iPhoneSystemCleaner.Services;

namespace iPhoneSystemCleaner.Views
{
    public partial class RestoreView : UserControl
    {
        private SshService? _ssh;

        public RestoreView()
        {
            InitializeComponent();
        }

        public void Initialize(SshService ssh)
        {
            _ssh = ssh;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBackups();
        }

        private void LoadBackups()
        {
            var backups = BackupManager.GetBackups();
            BackupList.ItemsSource = backups;
            
            if (backups.Count == 0)
            {
                TbEmpty.Visibility = Visibility.Visible;
                BackupList.Visibility = Visibility.Collapsed;
            }
            else
            {
                TbEmpty.Visibility = Visibility.Collapsed;
                BackupList.Visibility = Visibility.Visible;
            }
        }

        private async void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (_ssh == null || !_ssh.IsConnected)
            {
                MessageBox.Show("Chưa kết nối với iPhone!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var btn = (Button)sender;
            string path = (string)btn.Tag;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn khôi phục bản sao lưu này?\nDữ liệu hiện tại trên iPhone (của các mục tương ứng) sẽ bị ghi đè.", 
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            LogPanel.Visibility = Visibility.Visible;
            TbLog.Text = "";
            btn.IsEnabled = false;

            var (ok, error) = await _ssh.UploadAndRestoreBackupAsync(path, AppendLog);

            if (!ok)
            {
                AppendLog($"\n❌ Lỗi: {error}");
            }

            btn.IsEnabled = true;
        }

        private void BtnDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            string path = (string)btn.Tag;

            var result = MessageBox.Show("Bạn có chắc muốn xóa file backup này khỏi máy tính?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                BackupManager.DeleteBackup(path);
                LoadBackups();
            }
        }

        private void AppendLog(string line)
        {
            Dispatcher.Invoke(() =>
            {
                TbLog.Text += line + "\n";
                LogScroll.ScrollToBottom();
            });
        }
    }
}
