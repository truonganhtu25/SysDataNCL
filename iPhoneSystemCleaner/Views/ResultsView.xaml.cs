using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iPhoneSystemCleaner.Models;
using iPhoneSystemCleaner.Services;

namespace iPhoneSystemCleaner.Views
{
    public partial class ResultsView : UserControl
    {
        private SshService? _ssh;
        private List<ScanCategory> _categories = new();
        private readonly List<CheckBox> _checkBoxes = new();
        private int _lastClickedIndex = -1;

        public event Action<long>? OnDeleteComplete;

        public ResultsView()
        {
            InitializeComponent();
        }

        public void Initialize(SshService ssh)
        {
            _ssh = ssh;
        }

        public void LoadResults(List<ScanCategory> categories)
        {
            _categories = categories;

            long totalSize = categories.Sum(c => c.SizeBytes);
            int safeCount = categories.Count(c => c.Safety == SafetyLevel.Safe);
            int cautionCount = categories.Count(c => c.Safety != SafetyLevel.Safe);

            TbResultSubtitle.Text = $"Đã quét {categories.Count} nhóm — Tổng {FormatSize(totalSize)} có thể giải phóng";
            TbTotalFound.Text = FormatSize(totalSize);
            TbSafetyCount.Text = $"{safeCount} / {cautionCount}";

            RenderCategories();
        }

        private void RenderCategories()
        {
            _checkBoxes.Clear();
            CategoriesPanel.Children.Clear();
            _lastClickedIndex = -1;

            IEnumerable<ScanCategory> sorted;
            int sortIndex = CmbSort?.SelectedIndex ?? 0;
            if (sortIndex == 1)
                sorted = _categories.OrderBy(c => c.Safety).ThenByDescending(c => c.SizeBytes);
            else if (sortIndex == 2)
                sorted = _categories.OrderBy(c => c.Name);
            else
                sorted = _categories.OrderByDescending(c => c.SizeBytes);

            foreach (var cat in sorted)
            {
                var row = BuildCategoryRow(cat);
                CategoriesPanel.Children.Add(row);
            }

            UpdateSummary();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_categories.Count > 0)
            {
                RenderCategories();
            }
        }

        private Border BuildCategoryRow(ScanCategory cat)
        {
            var card = new Border
            {
                Margin = new Thickness(0, 0, 0, 8),
                Background = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x26)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x42)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(16, 12, 16, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Checkbox
            var chk = new CheckBox
            {
                Style = FindResource("DarkCheckBox") as Style,
                IsChecked = cat.IsSelected,
                IsEnabled = cat.SizeBytes > 0,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0),
                Tag = cat
            };
            
            int index = _checkBoxes.Count;
            chk.Click += (s, e) =>
            {
                if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift) && _lastClickedIndex != -1)
                {
                    int min = Math.Min(_lastClickedIndex, index);
                    int max = Math.Max(_lastClickedIndex, index);
                    bool targetState = chk.IsChecked == true;
                    
                    for (int i = min; i <= max; i++)
                    {
                        if (_checkBoxes[i].IsEnabled)
                        {
                            _checkBoxes[i].IsChecked = targetState;
                            ((ScanCategory)_checkBoxes[i].Tag!).IsSelected = targetState;
                        }
                    }
                }
                else
                {
                    _lastClickedIndex = index;
                    cat.IsSelected = chk.IsChecked == true;
                }
            };
            
            chk.Checked += (s, e) => UpdateSummary();
            chk.Unchecked += (s, e) => UpdateSummary();
            _checkBoxes.Add(chk);
            Grid.SetColumn(chk, 0);
            grid.Children.Add(chk);

            // Icon + name + description
            var namePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 0, 0) };
            var nameRow = new StackPanel { Orientation = Orientation.Horizontal };

            var iconTb = new TextBlock
            {
                Text = cat.Icon,
                FontSize = 18,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            nameRow.Children.Add(iconTb);

            var nameTb = new TextBlock
            {
                Text = cat.Name,
                Foreground = cat.SizeBytes > 0
                    ? new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xFF))
                    : new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x77)),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            nameRow.Children.Add(nameTb);
            namePanel.Children.Add(nameRow);

            var descTb = new TextBlock
            {
                Text = cat.Description,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0xAA)),
                FontSize = 11,
                Margin = new Thickness(28, 3, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            namePanel.Children.Add(descTb);

            // Paths preview
            var pathsTb = new TextBlock
            {
                Text = string.Join("\n", cat.Paths.Take(2)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x66)),
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(28, 3, 0, 0)
            };
            namePanel.Children.Add(pathsTb);

            Grid.SetColumn(namePanel, 1);
            grid.Children.Add(namePanel);

            // Safety badge
            var safetyBadge = new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(16, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(0x22,
                    Convert.ToByte(cat.SafetyColor[1..3], 16),
                    Convert.ToByte(cat.SafetyColor[3..5], 16),
                    Convert.ToByte(cat.SafetyColor[5..7], 16)))
            };
            var safetyTb = new TextBlock
            {
                Text = cat.SafetyText,
                Foreground = (Brush)new BrushConverter().ConvertFrom(cat.SafetyColor)!,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
            safetyBadge.Child = safetyTb;
            Grid.SetColumn(safetyBadge, 2);
            grid.Children.Add(safetyBadge);

            // Size
            var sizePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Width = 80 };
            var sizeTb = new TextBlock
            {
                Text = cat.SizeBytes > 0 ? cat.SizeDisplay : "Trống",
                Foreground = cat.SizeBytes > 1024 * 1024 * 50
                    ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                    : cat.SizeBytes > 1024 * 1024 * 5
                        ? new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B))
                        : new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0xAA)),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            sizePanel.Children.Add(sizeTb);
            Grid.SetColumn(sizePanel, 3);
            grid.Children.Add(sizePanel);

            card.Child = grid;

            // Hover effect
            card.MouseEnter += (s, e) =>
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60));
            card.MouseLeave += (s, e) =>
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x42));

            return card;
        }

        private void UpdateSummary()
        {
            var selected = _checkBoxes
                .Where(c => c.IsChecked == true && c.Tag is ScanCategory sc && sc.SizeBytes > 0)
                .Select(c => (ScanCategory)c.Tag!)
                .ToList();

            long totalSelected = selected.Sum(c => c.SizeBytes);
            TbSelectedCount.Text = $"{selected.Count} mục";
            TbSelectedSize.Text = FormatSize(totalSelected);

            BtnDelete.IsEnabled = selected.Count > 0;
            UpdateBtnDeleteText(selected.Count);
            BtnDeleteSubtext.Text = $"{selected.Count} mục · {FormatSize(totalSelected)}";
        }

        private void UpdateBtnDeleteText(int count)
        {
            if (count == 0)
            {
                BtnDeleteText.Text = "Chọn mục để xóa";
                return;
            }

            if (ChckEnableBackup?.IsChecked == true)
            {
                BtnDeleteText.Text = "Sao lưu & Xóa";
            }
            else
            {
                BtnDeleteText.Text = "Chỉ Xóa (Nguy hiểm)";
            }
        }

        private void ChckEnableBackup_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (BtnDeleteText != null)
            {
                UpdateSummary();
            }
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var chk in _checkBoxes.Where(c => c.IsEnabled))
            {
                chk.IsChecked = true;
                if (chk.Tag is ScanCategory cat) cat.IsSelected = true;
            }
        }

        private void BtnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var chk in _checkBoxes)
            {
                chk.IsChecked = false;
                if (chk.Tag is ScanCategory cat) cat.IsSelected = false;
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_ssh == null || !_ssh.IsConnected) return;

            var toDelete = _checkBoxes
                .Where(c => c.IsChecked == true && c.Tag is ScanCategory sc && sc.SizeBytes > 0)
                .Select(c => (ScanCategory)c.Tag!)
                .ToList();

            if (toDelete.Count == 0) return;

            long totalSize = toDelete.Sum(c => c.SizeBytes);

            // Confirmation dialog
            bool hasRisky = toDelete.Any(c => c.Safety == SafetyLevel.Risky);
            string msg = $"Bạn sắp xóa {toDelete.Count} nhóm file ({FormatSize(totalSize)}).\n\n";
            if (hasRisky)
                msg += "⚠️ CẢNH BÁO: Một số mục bạn chọn có mức độ RỦI RO — có thể xóa nhầm dữ liệu ứng dụng đang dùng!\n\n";
            msg += "Tiếp tục?";

            var result = MessageBox.Show(msg, "Xác nhận xóa",
                MessageBoxButton.YesNo,
                hasRisky ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // Start deletion (and backup)
            BtnDelete.IsEnabled = false;
            DeleteLogPanel.Visibility = Visibility.Visible;
            DeleteProgressBar.Visibility = Visibility.Visible;
            TbDeleteLog.Text = "";

            if (ChckEnableBackup.IsChecked == true)
            {
                AppendDeleteLog("📦 Đang thực hiện sao lưu trước khi xóa...");
                
                var allPathsToBackup = new System.Collections.Generic.List<string>();
                foreach (var cat in toDelete)
                {
                    allPathsToBackup.AddRange(cat.Paths);
                }

                Services.BackupManager.EnsureBackupDirectory();
                string destPath = System.IO.Path.Combine(Services.BackupManager.BackupDirectory, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.tar.gz");

                var backupResult = await _ssh.CreateAndDownloadBackupAsync(allPathsToBackup, destPath, AppendDeleteLog);
                if (!backupResult.success)
                {
                    AppendDeleteLog($"❌ Lỗi sao lưu: {backupResult.error}");
                    AppendDeleteLog("⚠️ Đã hủy thao tác xóa để bảo vệ dữ liệu.");
                    BtnDelete.IsEnabled = true;
                    return;
                }
            }

            long freedTotal = 0;
            int done = 0;

            foreach (var cat in toDelete)
            {
                AppendDeleteLog($"\n[{done + 1}/{toDelete.Count}] {cat.Icon} {cat.Name}");

                foreach (var path in cat.Paths)
                {
                    var (ok, msg2) = await _ssh.DeleteContentsAsync(path, AppendDeleteLog);
                }

                // Estimate freed (actual may differ slightly)
                freedTotal += cat.SizeBytes;
                done++;

                Dispatcher.Invoke(() =>
                {
                    DeleteProgressBar.Value = (double)done / toDelete.Count * 100;
                });

                // Uncheck the category
                Dispatcher.Invoke(() =>
                {
                    var chk = _checkBoxes.FirstOrDefault(c => c.Tag == cat);
                    if (chk != null) chk.IsChecked = false;
                    cat.SizeBytes = 0;
                    cat.IsScanned = false;
                });
            }

            AppendDeleteLog($"\n🎉 Hoàn tất! Đã xóa {done} nhóm ~ {FormatSize(freedTotal)} được giải phóng.");

            Dispatcher.Invoke(() =>
            {
                BtnDelete.IsEnabled = false;
                DeleteProgressBar.Value = 100;
                UpdateSummary();
            });

            OnDeleteComplete?.Invoke(freedTotal);
        }

        private void AppendDeleteLog(string line)
        {
            Dispatcher.Invoke(() =>
            {
                TbDeleteLog.Text += line + "\n";
                DeleteLogScroll.ScrollToBottom();
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
