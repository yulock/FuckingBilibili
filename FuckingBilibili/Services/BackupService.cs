using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FuckingBilibili.Services
{
    /// <summary>
    /// 备份恢复服务
    /// </summary>
    public class BackupService
    {
        private const string BackupFolderName = "ConfigBackups";
        private const string ConfigFileName = "config.ini";

        /// <summary>
        /// 获取备份目录路径
        /// </summary>
        private string GetBackupDirectory(string gamePath)
        {
            string backupDir = Path.Combine(gamePath, BackupFolderName);
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);
            return backupDir;
        }

        /// <summary>
        /// 创建配置文件备份
        /// </summary>
        /// <param name="gamePath">游戏目录路径</param>
        /// <returns>备份文件名</returns>
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

        /// <summary>
        /// 获取所有备份文件列表
        /// </summary>
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

        /// <summary>
        /// 恢复指定备份
        /// </summary>
        public void RestoreBackup(string gamePath, string backupFileName)
        {
            try
            {
                string backupDir = GetBackupDirectory(gamePath);
                string backupPath = Path.Combine(backupDir, backupFileName);

                if (!File.Exists(backupPath))
                    throw new FileNotFoundException("备份文件不存在");

                string configPath = Path.Combine(gamePath, ConfigFileName);

                // 先备份当前配置
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string currentBackup = Path.Combine(backupDir, $"config_auto_{timestamp}.ini");
                if (File.Exists(configPath))
                {
                    File.Copy(configPath, currentBackup, true);
                }

                // 恢复备份
                File.Copy(backupPath, configPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"恢复备份失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除备份文件
        /// </summary>
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

        /// <summary>
        /// 从备份文件名或配置文件中获取服务器类型
        /// </summary>
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

    /// <summary>
    /// 备份信息
    /// </summary>
    public class BackupInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }
        public long Size { get; set; }

        public string DisplayName => $"{FileName} ({CreateTime:MM-dd HH:mm})";
    }
}
