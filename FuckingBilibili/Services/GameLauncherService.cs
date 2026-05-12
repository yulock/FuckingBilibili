using System;
using System.Diagnostics;
using System.IO;

namespace FuckingBilibili.Services
{
    public class GameLauncherService
    {
        private const string GameExeName = "YuanShen.exe";
        private const string LauncherExeName = "launcher.exe";

        public bool LaunchGame(string gamePath)
        {
            try
            {
                string exePath = Path.Combine(gamePath, GameExeName);
                if (!File.Exists(exePath))
                    throw new FileNotFoundException("游戏可执行文件不存在");

                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = gamePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"启动游戏失败: {ex.Message}");
            }
        }

        public bool IsGameRunning()
        {
            var processes = Process.GetProcessesByName("YuanShen");
            return processes.Length > 0;
        }

        public Process? GetGameProcess()
        {
            var processes = Process.GetProcessesByName("YuanShen");
            return processes.Length > 0 ? processes[0] : null;
        }
    }
}
