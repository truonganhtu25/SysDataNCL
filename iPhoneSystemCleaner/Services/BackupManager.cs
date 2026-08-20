using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iPhoneSystemCleaner.Models;

namespace iPhoneSystemCleaner.Services
{
    public class BackupManager
    {
        public static string BackupDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        public static void EnsureBackupDirectory()
        {
            if (!Directory.Exists(BackupDirectory))
            {
                Directory.CreateDirectory(BackupDirectory);
            }
        }

        public static List<BackupRecord> GetBackups()
        {
            EnsureBackupDirectory();
            
            var dir = new DirectoryInfo(BackupDirectory);
            var files = dir.GetFiles("*.tar.gz").OrderByDescending(f => f.CreationTime);
            
            var list = new List<BackupRecord>();
            foreach (var file in files)
            {
                list.Add(new BackupRecord
                {
                    FileName = file.Name,
                    FullPath = file.FullName,
                    CreatedAt = file.CreationTime,
                    SizeBytes = file.Length
                });
            }
            return list;
        }

        public static void DeleteBackup(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
