using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FuckingBilibili.Services
{
    /// <summary>
    /// 游戏路径检测服务
    /// </summary>
    public class GamePathService
    {
        private const string GameFolderName = "Genshin Impact Game";

        /// <summary>
        /// 常见的原神安装路径
        /// </summary>
        private readonly List<string> _commonPaths = new List<string>
        {
            @"C:\Program Files\Genshin Impact\Genshin Impact Game",
            @"D:\Genshin Impact\Genshin Impact Game",
            @"E:\Genshin Impact\Genshin Impact Game",
            @"F:\Genshin Impact\Genshin Impact Game",
            @"C:\Games\Genshin Impact\Genshin Impact Game",
            @"D:\Games\Genshin Impact\Genshin Impact Game",
            @"E:\Games\Genshin Impact\Genshin Impact Game",
            @"F:\Games\Genshin Impact\Genshin Impact Game",
            @"C:\Program Files (x86)\Genshin Impact\Genshin Impact Game",
        };

        /// <summary>
        /// 自动检测游戏目录
        /// </summary>
        /// <returns>游戏目录路径，未找到返回null</returns>
        public string? AutoDetectGamePath()
        {
            // 1. 检查常见路径
            foreach (var path in _commonPaths)
            {
                if (IsValidGamePath(path))
                    return path;
            }

            // 2. 遍历所有磁盘查找
            var drives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                .Select(d => d.RootDirectory.FullName);

            foreach (var drive in drives)
            {
                var foundPath = SearchGameFolder(drive);
                if (foundPath != null)
                    return foundPath;
            }

            return null;
        }

        /// <summary>
        /// 验证路径是否为有效的游戏目录
        /// </summary>
        private bool IsValidGamePath(string path)
        {
            if (!Directory.Exists(path))
                return false;

            // 检查是否包含关键文件
            string configPath = Path.Combine(path, "config.ini");
            string exePath = Path.Combine(path, "YuanShen.exe");

            return File.Exists(configPath) && File.Exists(exePath);
        }

        /// <summary>
        /// 递归搜索游戏文件夹
        /// </summary>
        private string? SearchGameFolder(string rootPath, int maxDepth = 3)
        {
            try
            {
                // 限制搜索深度以提高性能
                return SearchDirectory(rootPath, GameFolderName, 0, maxDepth);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 递归搜索目录
        /// </summary>
        private string? SearchDirectory(string currentPath, string targetFolder, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth)
                return null;

            try
            {
                // 检查当前目录
                if (Path.GetFileName(currentPath).Equals(targetFolder, StringComparison.OrdinalIgnoreCase))
                {
                    if (IsValidGamePath(currentPath))
                        return currentPath;
                }

                // 检查子目录
                foreach (var subDir in Directory.GetDirectories(currentPath))
                {
                    // 跳过系统目录
                    var dirName = Path.GetFileName(subDir);
                    if (dirName.StartsWith("$") || 
                        dirName.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("ProgramData", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Users", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (dirName.Equals(targetFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsValidGamePath(subDir))
                            return subDir;
                    }

                    var result = SearchDirectory(subDir, targetFolder, currentDepth + 1, maxDepth);
                    if (result != null)
                        return result;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无权限访问的目录
            }
            catch (DirectoryNotFoundException)
            {
                // 忽略不存在的目录
            }

            return null;
        }

        /// <summary>
        /// 验证指定路径是否为有效游戏目录
        /// </summary>
        public bool ValidatePath(string path)
        {
            return IsValidGamePath(path);
        }
    }
}
