using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace iPhoneSystemCleaner.Services
{
    public class SshService : IDisposable
    {
        private SshClient? _sshClient;
        private ScpClient? _scpClient;
        private bool _disposed = false;

        public bool IsConnected => _sshClient?.IsConnected == true && _scpClient?.IsConnected == true;

        public event Action<string>? LogMessage;

        /// <summary>Kết nối SSH đến iPhone</summary>
        public async Task<(bool success, string message)> ConnectAsync(
            string host, string username, string password, int port = 22)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _sshClient?.Dispose();
                    var connectionInfo = new ConnectionInfo(host, port, username,
                        new PasswordAuthenticationMethod(username, password))
                    {
                        Timeout = TimeSpan.FromSeconds(15)
                    };
                    _sshClient = new SshClient(connectionInfo);
                    _sshClient.Connect();
                    
                    _scpClient?.Dispose();
                    _scpClient = new ScpClient(connectionInfo);
                    _scpClient.Connect();

                    Log("✅ Kết nối SSH/SCP thành công");
                    return (true, "Kết nối thành công");
                }
                catch (SshAuthenticationException)
                {
                    return (false, "Sai tên đăng nhập hoặc mật khẩu");
                }
                catch (System.Net.Sockets.SocketException)
                {
                    return (false, "Không thể kết nối đến địa chỉ IP này. Kiểm tra lại IP và cổng SSH.");
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi kết nối: {ex.Message}");
                }
            });
        }

        /// <summary>Chạy lệnh shell trên iPhone và trả về output</summary>
        public async Task<string> RunCommandAsync(string command, int timeoutSeconds = 60)
        {
            if (!IsConnected) throw new InvalidOperationException("Chưa kết nối SSH");

            return await Task.Run(() =>
            {
                try
                {
                    using var cmd = _sshClient!.CreateCommand(command);
                    cmd.CommandTimeout = TimeSpan.FromSeconds(timeoutSeconds);
                    var result = cmd.Execute();
                    return result?.Trim() ?? "";
                }
                catch (Exception ex)
                {
                    Log($"⚠️ Lỗi lệnh '{command}': {ex.Message}");
                    return "";
                }
            });
        }

        /// <summary>Lấy thông tin thiết bị</summary>
        public async Task<DeviceInfo> GetDeviceInfoAsync()
        {
            var info = new DeviceInfo();
            try
            {
                // iOS version
                info.IosVersion = await RunCommandAsync("sw_vers -productVersion 2>/dev/null || cat /System/Library/CoreServices/SystemVersion.plist | grep -A1 ProductVersion | tail -1 | sed 's/.*<string>//;s/<\\/string>//'");

                // Hostname
                info.Hostname = await RunCommandAsync("hostname");

                // Disk usage (tổng / đã dùng / còn trống)
                var dfOut = await RunCommandAsync("df -k / | tail -1");
                if (!string.IsNullOrEmpty(dfOut))
                {
                    var parts = dfOut.Split(new char[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        info.DiskTotalKb = long.TryParse(parts[1], out var t) ? t : 0;
                        info.DiskUsedKb = long.TryParse(parts[2], out var u) ? u : 0;
                        info.DiskFreeKb = long.TryParse(parts[3], out var f) ? f : 0;
                    }
                }

                // Detect jailbreak type
                var hasVarJb = await RunCommandAsync("test -d /var/jb && echo 'yes' || echo 'no'");
                info.IsRootless = hasVarJb.Trim() == "yes";
                info.JailbreakType = info.IsRootless ? "Rootless (/var/jb/)" : "Rootful (/)";
            }
            catch { }
            return info;
        }

        /// <summary>Tính dung lượng của một đường dẫn trên iPhone</summary>
        public async Task<long> GetDirectorySizeAsync(string path)
        {
            // du -sk returns kilobytes
            var result = await RunCommandAsync(
                $"du -sk \"{path}\" 2>/dev/null | awk '{{print $1}}'",
                timeoutSeconds: 120);
            return long.TryParse(result.Trim(), out var kb) ? kb * 1024 : 0;
        }

        /// <summary>Liệt kê các thư mục con cấp 1 với kích thước</summary>
        public async Task<List<(string path, long sizeBytes)>> ListSubdirsWithSizeAsync(string basePath)
        {
            var result = new List<(string, long)>();
            var output = await RunCommandAsync(
                $"du -sk \"{basePath}\"/* 2>/dev/null | sort -rn",
                timeoutSeconds: 120);

            if (string.IsNullOrEmpty(output)) return result;

            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('\t');
                if (parts.Length >= 2 && long.TryParse(parts[0].Trim(), out var kb))
                {
                    result.Add((parts[1].Trim(), kb * 1024));
                }
            }
            return result;
        }

        /// <summary>Xóa nội dung bên trong thư mục (không xóa thư mục cha)</summary>
        public async Task<(bool success, string message)> DeleteContentsAsync(
            string path, Action<string>? onLog = null)
        {
            try
            {
                onLog?.Invoke($"🗑️ Xóa nội dung: {path}");
                var result = await RunCommandAsync(
                    $"rm -rf \"{path}\"/* 2>&1 && echo 'OK' || echo 'FAIL'",
                    timeoutSeconds: 120);
                if (result.Contains("OK"))
                {
                    onLog?.Invoke($"  ✅ Xong: {path}");
                    return (true, "OK");
                }
                else
                {
                    onLog?.Invoke($"  ⚠️ Một số file không xóa được: {path}");
                    return (true, "Partial");
                }
            }
            catch (Exception ex)
            {
                onLog?.Invoke($"  ❌ Lỗi: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>Xóa toàn bộ thư mục và tạo lại rỗng</summary>
        public async Task<(bool success, string message)> DeleteAndRecreateDirAsync(
            string path, Action<string>? onLog = null)
        {
            try
            {
                onLog?.Invoke($"🗑️ Xóa & tạo lại: {path}");
                await RunCommandAsync($"rm -rf \"{path}\" && mkdir -p \"{path}\"", 120);
                onLog?.Invoke($"  ✅ Xong");
                return (true, "OK");
            }
            catch (Exception ex)
            {
                onLog?.Invoke($"  ❌ Lỗi: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public void Disconnect()
        {
            _sshClient?.Disconnect();
        }

        private void Log(string msg) => LogMessage?.Invoke(msg);

        public void Dispose()
        {
            if (!_disposed)
            {
                _sshClient?.Dispose();
                _scpClient?.Dispose();
                _disposed = true;
            }
        }

        // ═══ BACKUP & RESTORE ═══

        public async Task<(bool success, string error)> CreateAndDownloadBackupAsync(List<string> paths, string localDestPath, Action<string> log)
        {
            if (!IsConnected || _scpClient == null) return (false, "Chưa kết nối SSH");

            return await Task.Run(() =>
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string tarFileName = $"backup_{timestamp}.tar.gz";
                    string tarFilePath = $"/var/tmp/{tarFileName}";

                    log($"[Backup] Đang nén {paths.Count} thư mục...");
                    
                    // Create space-separated string of paths
                    string pathsArg = string.Join(" ", paths.ConvertAll(p => $"\"{p}\""));
                    
                    // tar command: -c create, -z gzip, -p preserve permissions, -P absolute paths
                    string cmdTar = $"tar -czpPf {tarFilePath} {pathsArg} 2>/dev/null";
                    var cmdResult = _sshClient!.RunCommand(cmdTar);
                    
                    if (cmdResult.ExitStatus != 0 && cmdResult.ExitStatus != 1) 
                    {
                        // Exit status 1 in tar usually means some files changed during read, which is common. >1 is error.
                        log($"[Backup] Cảnh báo khi nén: {cmdResult.Error}");
                    }

                    log($"[Backup] Nén xong. Đang tải về máy tính (file: {tarFileName})...");

                    // Download via SCP
                    var fileInfo = new System.IO.FileInfo(localDestPath);
                    fileInfo.Directory?.Create();

                    _scpClient.Download(tarFilePath, fileInfo);

                    log($"[Backup] Tải xong. Đang dọn dẹp file tạm trên iPhone...");
                    _sshClient.RunCommand($"rm -f {tarFilePath}");

                    log($"[Backup] Hoàn tất sao lưu vào: {localDestPath}");
                    return (true, string.Empty);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            });
        }

        public async Task<(bool success, string error)> UploadAndRestoreBackupAsync(string localBackupPath, Action<string> log)
        {
            if (!IsConnected || _scpClient == null) return (false, "Chưa kết nối SSH");

            return await Task.Run(() =>
            {
                try
                {
                    string tarFileName = System.IO.Path.GetFileName(localBackupPath);
                    string tarFilePath = $"/var/tmp/{tarFileName}";

                    log($"[Restore] Đang tải bản sao lưu lên iPhone ({tarFileName})...");
                    
                    var fileInfo = new System.IO.FileInfo(localBackupPath);
                    _scpClient.Upload(fileInfo, tarFilePath);

                    log($"[Restore] Tải lên xong. Đang giải nén và ghi đè dữ liệu...");
                    // tar command: -x extract, -z gzip, -p preserve permissions, -P absolute paths
                    string cmdTar = $"tar -xzpPf {tarFilePath}";
                    var cmdResult = _sshClient!.RunCommand(cmdTar);

                    if (cmdResult.ExitStatus != 0)
                    {
                        log($"[Restore] Cảnh báo khi giải nén: {cmdResult.Error}");
                    }

                    log($"[Restore] Đang dọn dẹp file tạm...");
                    _sshClient.RunCommand($"rm -f {tarFilePath}");

                    log($"[Restore] Khôi phục thành công!");
                    return (true, string.Empty);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            });
        }
    }

    public class DeviceInfo
    {
        public string IosVersion { get; set; } = "Không rõ";
        public string Hostname { get; set; } = "iPhone";
        public long DiskTotalKb { get; set; }
        public long DiskUsedKb { get; set; }
        public long DiskFreeKb { get; set; }
        public bool IsRootless { get; set; }
        public string JailbreakType { get; set; } = "Không rõ";

        public string DiskTotalDisplay => FormatKb(DiskTotalKb);
        public string DiskUsedDisplay => FormatKb(DiskUsedKb);
        public string DiskFreeDisplay => FormatKb(DiskFreeKb);
        public double UsedPercent => DiskTotalKb > 0 ? (double)DiskUsedKb / DiskTotalKb * 100 : 0;

        private static string FormatKb(long kb)
        {
            if (kb < 1024) return $"{kb} KB";
            if (kb < 1024 * 1024) return $"{kb / 1024.0:F1} MB";
            return $"{kb / 1024.0 / 1024.0:F2} GB";
        }
    }
}
