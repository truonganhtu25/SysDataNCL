using System.Collections.Generic;
using iPhoneSystemCleaner.Models;

namespace iPhoneSystemCleaner.Services
{
    /// <summary>
    /// Định nghĩa tất cả 12 nhóm file có thể quét và xóa.
    /// Hỗ trợ cả rootless (/var/jb/) và rootful (/).
    /// </summary>
    public static class ScanCategoryDefinitions
    {
        public static List<ScanCategory> GetCategories(bool isRootless)
        {
            // Prefix cho thư mục jailbreak
            // Với rootless, các binary jailbreak ở /var/jb/
            // Nhưng user data vẫn ở /var/mobile/
            string jbPrefix = isRootless ? "/var/jb" : "";

            return new List<ScanCategory>
            {
                new ScanCategory
                {
                    Id = "crash_reports",
                    Name = "Crash Reports",
                    Icon = "💥",
                    Description = "Báo cáo lỗi ứng dụng — sinh ra khi app crash, không ảnh hưởng đến hoạt động bình thường",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Logs/CrashReporter",
                        "/var/log/asl",
                        $"{jbPrefix}/var/log/asl"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "system_logs",
                    Name = "System Logs",
                    Icon = "📋",
                    Description = "Log hệ thống tích lũy theo thời gian — hoàn toàn an toàn để xóa",
                    Paths = new[]
                    {
                        "/var/log",
                        $"{jbPrefix}/var/log"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "temp_files",
                    Name = "Temp Files",
                    Icon = "🗑️",
                    Description = "File tạm thời — iOS sẽ tự tạo lại khi cần",
                    Paths = new[]
                    {
                        "/private/var/tmp",
                        "/var/tmp"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "battery_archives",
                    Name = "Battery Archives",
                    Icon = "🔋",
                    Description = "Lưu trữ thống kê pin theo thời gian — xóa không ảnh hưởng đến chức năng",
                    Paths = new[]
                    {
                        "/var/mobile/Library/BatteryLife/Archives"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "safari_cache",
                    Name = "Safari Cache & WebKit",
                    Icon = "🌐",
                    Description = "Cache trình duyệt Safari và WebKit — an toàn, Safari sẽ load lại khi cần",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Caches/com.apple.WebKit",
                        "/var/mobile/Library/Caches/com.apple.Safari",
                        "/var/mobile/Library/Caches/com.apple.SafariShared"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "stuck_cache",
                    Name = "Stuck App Cache (Deathrow)",
                    Icon = "🧊",
                    Description = "Cache bị 'kẹt' từ app đã gỡ — hệ thống đánh dấu để xóa nhưng không xóa được",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Caches/com.apple.CacheDeleteAppContainerCaches.deathrow"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "diagnostic_logs",
                    Name = "Diagnostic & Sysdiagnose",
                    Icon = "🩺",
                    Description = "File chẩn đoán hệ thống — chỉ dùng cho debug, hoàn toàn an toàn để xóa",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Logs/Sysdiagnose",
                        "/var/mobile/Library/Logs/itunesstored",
                        "/var/mobile/Library/Logs/mDNSResponder"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "spotlight_cache",
                    Name = "Spotlight Index Cache",
                    Icon = "🔍",
                    Description = "Cache index tìm kiếm Spotlight — xóa an toàn, iOS tự build lại (có thể mất vài phút)",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Caches/com.apple.Spotlight"
                    },
                    Safety = SafetyLevel.Caution
                },

                new ScanCategory
                {
                    Id = "ota_cache",
                    Name = "OTA Update Cache",
                    Icon = "📦",
                    Description = "File cập nhật iOS đã tải về nhưng chưa cài — an toàn để xóa nếu không muốn update",
                    Paths = new[]
                    {
                        "/var/MobileAsset/AssetsV2",
                        "/var/MobileAsset/Assets"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "package_manager_cache",
                    Name = "Sileo / Cydia Cache",
                    Icon = "📱",
                    Description = "Cache của trình quản lý package jailbreak — an toàn, sẽ tải lại khi cần",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Caches/com.saurik.Cydia",
                        "/var/mobile/Library/Caches/xyz.willy.Zebra",
                        $"{jbPrefix}/var/cache/apt",
                        "/var/cache/apt"
                    },
                    Safety = SafetyLevel.Safe
                },

                new ScanCategory
                {
                    Id = "app_cache_subfolders",
                    Name = "App Cache (Chọn lọc)",
                    Icon = "🗂️",
                    Description = "Cache ứng dụng trong /var/mobile/Library/Caches — liệt kê từng thư mục con để chọn lọc",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Caches"
                    },
                    Safety = SafetyLevel.Caution
                },

                new ScanCategory
                {
                    Id = "orphaned_containers",
                    Name = "App Leftover Data",
                    Icon = "🏚️",
                    Description = "Dữ liệu còn sót lại của app đã gỡ trong /var/mobile/Containers — CẢNH BÁO: có thể xóa nhầm app đang dùng",
                    Paths = new[]
                    {
                        "/var/mobile/Containers/Data/Application"
                    },
                    Safety = SafetyLevel.Risky
                },

                new ScanCategory
                {
                    Id = "apple_ai_data",
                    Name = "Apple AI & ML Data",
                    Icon = "🧠",
                    Description = "Dữ liệu huấn luyện AI của Apple (CoreDuet, MachineLearning) — CẢNH BÁO: Xóa sẽ khiến iPhone phải học lại thói quen từ đầu, tốn pin và máy có thể chậm đi",
                    Paths = new[]
                    {
                        "/var/mobile/Library/CoreDuet",
                        "/var/mobile/Library/MachineLearning",
                        "/var/mobile/Library/PersonalizationPortrait"
                    },
                    Safety = SafetyLevel.Risky
                },

                new ScanCategory
                {
                    Id = "wallpaper_posters",
                    Name = "Wallpaper & Posters Cache",
                    Icon = "🖼️",
                    Description = "Bộ nhớ đệm hình nền và Lockscreen (iOS 16+) — Xóa sẽ reset các màn hình khóa tùy chỉnh về mặc định, giúp dọn dẹp các ảnh cũ bị kẹt",
                    Paths = new[]
                    {
                        "/var/mobile/Library/Application Support/com.apple.PosterBoard"
                    },
                    Safety = SafetyLevel.Caution
                }
            };
        }
    }
}
