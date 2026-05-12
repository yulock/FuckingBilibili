using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FuckingBilibili.Services
{
    public class BackupService
    {
        private const string BackupFolderName = "ConfigBackups";
        private const string ConfigFileName = "config.ini";

        private string GetBackupDirectory(string gamePath)
        {
            string backupDir = Path.Combine(gamePath, BackupFolderName);
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);
            return backupDir;
        }

        public string CreateBackup(string gamePath)
        {
            try
            {
                string configPath = Path.Combine(gamePath, ConfigFileName);
                if (!File.Exists(configPath))
                    throw new FileNotFoundException("配置文件不存在");

                string backupDir = GetBackupDirectory(gamePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string serverType = GetCurrentServerType(gamePath);
                string backupFileName = $"config_{serverType}_{timestamp}.ini";
                string backupPath = Path.Combine(backupDir, backupFileName);

                File.Copy(configPath, backupPath, true);
                return backupFileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"创建备份失败: {ex.Message}");
            }
        }

        public List<BackupInfo> GetBackupList(string gamePath)
        {
            var backups = new List<BackupInfo>();
            string backupDir = Path.Combine(gamePath, BackupFolderName);

            if (!Directory.Exists(backupDir))
                return backups;

            var files = Directory.GetFiles(backupDir, "config_*.ini")
                .OrderByDescending(f => new FileInfo(f).CreationTime);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupInfo
                {
                    FileName = Path.GetFileName(file),
                    FullPath = file,
                    CreateTime = fileInfo.CreationTime,
                    Size = fileInfo.Length
                });
            }

            return backups;
        }

        public void RestoreBackup(string gamePath, string backupFileName)
        {
            try
            {
                string backupDir = GetBackupDirectory(gamePath);
                string backupPath = Path.Combine(backupDir, backupFileName);

                if (!File.Exists(backupPath))
                    throw new FileNotFoundException("备份文件不存在");

                string configPath = Path.Combine(gamePath, ConfigFileName);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string currentBackup = Path.Combine(backupDir, $"config_auto_{timestamp}.ini");
                if (File.Exists(configPath))
                {
                    File.Copy(configPath, currentBackup, true);
                }

                File.Copy(backupPath, configPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"恢复备份失败: {ex.Message}");
            }
        }

        public void DeleteBackup(string gamePath, string backupFileName)
        {
            try
            {
                string backupDir = GetBackupDirectory(gamePath);
                string backupPath = Path.Combine(backupDir, backupFileName);

                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"删除备份失败: {ex.Message}");
            }
        }

        private string GetCurrentServerType(string gamePath)
        {
            try
            {
                string configPath = Path.Combine(gamePath, ConfigFileName);
                if (!File.Exists(configPath))
                    return "unknown";

                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("game_biz", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            var value = parts[1].Trim();
                            if (value == "hk4e_bilibili")
                                return "bilibili";
                            else if (value == "hk4e_cn")
                                return "official";
                        }
                    }
                }
                return "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }

    public class BackupInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }
        public long Size { get; set; }

        public string DisplayName => $"{FileName} ({CreateTime:MM-dd HH:mm})";
    }
}
