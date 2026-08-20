using System;

namespace iPhoneSystemCleaner.Models
{
    public class ScanCategory
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Description { get; set; } = "";
        public string[] Paths { get; set; } = Array.Empty<string>();
        public SafetyLevel Safety { get; set; } = SafetyLevel.Safe;
        public long SizeBytes { get; set; } = 0;
        public bool IsSelected { get; set; } = false;
        public bool IsScanned { get; set; } = false;
        public string SizeDisplay => FormatSize(SizeBytes);
        public string SafetyText => Safety switch
        {
            SafetyLevel.Safe => "An toàn",
            SafetyLevel.Caution => "Cẩn thận",
            SafetyLevel.Risky => "Rủi ro",
            _ => "Không rõ"
        };
        public string SafetyColor => Safety switch
        {
            SafetyLevel.Safe => "#22C55E",
            SafetyLevel.Caution => "#F59E0B",
            SafetyLevel.Risky => "#EF4444",
            _ => "#888888"
        };

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / 1024.0 / 1024.0:F1} MB";
            return $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
        }
    }

    public enum SafetyLevel
    {
        Safe,
        Caution,
        Risky
    }
}
