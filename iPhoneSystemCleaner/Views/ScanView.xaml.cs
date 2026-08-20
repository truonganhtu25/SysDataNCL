using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using iPhoneSystemCleaner.Models;
using iPhoneSystemCleaner.Services;

namespace iPhoneSystemCleaner.Views
{
    public partial class ScanView : UserControl
    {
        private SshService? _ssh;
        private CancellationTokenSource? _cts;

        public event Action? OnScanComplete;
        public List<ScanCategory> ScannedCategories { get; private set; } = new();

        public ScanView()
        {
            InitializeComponent();
        }

        public void Initialize(SshService ssh)
        {
            _ssh = ssh;
        }

        private async void BtnStartScan_Click(object sender, RoutedEventArgs e)
        {
            if (_ssh == null || !_ssh.IsConnected)
            {
                AppendLog("❌ Chưa kết nối SSH. Vui lòng quay lại tab Kết nối.");
                return;
            }

            _cts = new CancellationTokenSource();
            BtnStartScan.IsEnabled = false;
            BtnCancelScan.IsEnabled = true;
            BtnScanText.Text = "Đang quét...";
            ScanProgressBar.Value = 0;
            TbLog.Text = "";
            ScannedCategories.Clear();

            AppendLog("root@iPhone:~$ Bắt đầu quét Dữ liệu Hệ thống...");
            AppendLog("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            try
            {
                // Get device info for disk display
                AppendLog("📊 Lấy thông tin ổ đĩa...");
                var devInfo = await _ssh.GetDeviceInfoAsync();
                Dispatcher.Invoke(() =>
                {
                    TbDiskTotal.Text = devInfo.DiskTotalDisplay;
                    TbDiskUsed.Text = devInfo.DiskUsedDisplay;
                });

                // Detect jailbreak type
                bool isRootless = devInfo.IsRootless;
                AppendLog($"🔍 Loại jailbreak: {devInfo.JailbreakType}");

                var categories = ScanCategoryDefinitions.GetCategories(isRootless);
                int total = categories.Count;
                long totalFoundBytes = 0;

                for (int i = 0; i < categories.Count; i++)
                {
                    if (_cts.Token.IsCancellationRequested) break;

                    var cat = categories[i];
                    AppendLog($"\n[{i + 1}/{total}] {cat.Icon} {cat.Name}");

                    long catSize = 0;
                    foreach (var path in cat.Paths)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        AppendLog($"      📁 {path}");
                        // Check if path exists
                        var exists = await _ssh.RunCommandAsync($"test -e \"{path}\" && echo 'yes' || echo 'no'");
                        if (exists.Trim() != "yes")
                        {
                            AppendLog($"         ⬜ Không tồn tại — bỏ qua");
                            continue;
                        }

                        var size = await _ssh.GetDirectorySizeAsync(path);
                        catSize += size;
                        AppendLog($"         📦 {FormatSize(size)}");
                    }

                    cat.SizeBytes = catSize;
                    cat.IsScanned = true;
                    totalFoundBytes += catSize;

                    Dispatcher.Invoke(() =>
                    {
                        ScanProgressBar.Value = (double)(i + 1) / total * 100;
                        TbScanStatus.Text = $"Đã quét {i + 1}/{total} nhóm...";
                        TbEstimatedFree.Text = FormatSize(totalFoundBytes);
                    });

                    ScannedCategories.Add(cat);
                }

                AppendLog("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                AppendLog($"✅ Quét hoàn tất! Tổng dữ liệu có thể xóa: {FormatSize(totalFoundBytes)}");
                AppendLog("📋 Chuyển sang tab 'Kết quả & Xóa' để chọn mục cần xóa.");

                Dispatcher.Invoke(() =>
                {
                    ScanProgressBar.Value = 100;
                    TbScanStatus.Text = $"✅ Hoàn tất — Tìm thấy {FormatSize(totalFoundBytes)} có thể giải phóng";

                    // Pass results to ResultsView
                    var mainWin = MainWindow.Instance;
                    if (mainWin != null)
                    {
                        mainWin.ViewResults.LoadResults(ScannedCategories);
                        OnScanComplete?.Invoke();
                    }
                });
            }
            catch (OperationCanceledException)
            {
                AppendLog("\n⏹ Đã dừng quét.");
            }
            catch (Exception ex)
            {
                AppendLog($"\n❌ Lỗi: {ex.Message}");
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    BtnStartScan.IsEnabled = true;
                    BtnCancelScan.IsEnabled = false;
                    BtnScanText.Text = "Quét lại";
                    BtnScanIcon.Text = "🔄  ";
                });
            }
        }

        private void BtnCancelScan_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private void AppendLog(string line)
        {
            Dispatcher.Invoke(() =>
            {
                TbLog.Text += line + "\n";
                LogScrollViewer.ScrollToBottom();
                TbScanStatus.Text = line.Length > 60 ? line[..57] + "..." : line;
            });
        }

        private static string FormatSize(long bytes)
        {
            if (bytes <= 0) return "0 B";
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / 1024.0 / 1024.0:F1} MB";
            return $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
        }
    }
}
