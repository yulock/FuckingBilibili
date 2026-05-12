using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FuckingBilibili.Services
{
    public class GamePathService
    {
        private const string GameFolderName = "Genshin Impact Game";

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

        public string? AutoDetectGamePath()
        {
            foreach (var path in _commonPaths)
            {
                if (IsValidGamePath(path))
                    return path;
            }

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

        private bool IsValidGamePath(string path)
        {
            if (!Directory.Exists(path))
                return false;

            string configPath = Path.Combine(path, "config.ini");
            string exePath = Path.Combine(path, "YuanShen.exe");

            return File.Exists(configPath) && File.Exists(exePath);
        }

        private string? SearchGameFolder(string rootPath, int maxDepth = 3)
        {
            try
            {
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

        private string? SearchDirectory(string currentPath, string targetFolder, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth)
                return null;

            try
            {
                if (Path.GetFileName(currentPath).Equals(targetFolder, StringComparison.OrdinalIgnoreCase))
                {
                    if (IsValidGamePath(currentPath))
                        return currentPath;
                }

                foreach (var subDir in Directory.GetDirectories(currentPath))
                {
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
            }
            catch (DirectoryNotFoundException)
            {
            }

            return null;
        }

        public bool ValidatePath(string path)
        {
            return IsValidGamePath(path);
        }
    }
}
