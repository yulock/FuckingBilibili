using System;
using System.Diagnostics;
using System.IO;

namespace FuckingBilibili.Services
{
    /// <summary>
    /// 游戏启动服务
    /// </summary>
    public class GameLauncherService
    {
        private const string GameExeName = "YuanShen.exe";
        private const string LauncherExeName = "launcher.exe";

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="gamePath">游戏目录路径</param>
        /// <returns>是否启动成功</returns>
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
                    Verb = "runas" // 以管理员权限运行（某些情况下需要）
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"启动游戏失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查游戏是否正在运行
        /// </summary>
        public bool IsGameRunning()
        {
            var processes = Process.GetProcessesByName("YuanShen");
            return processes.Length > 0;
        }

        /// <summary>
        /// 获取游戏进程
        /// </summary>
        public Process? GetGameProcess()
        {
            var processes = Process.GetProcessesByName("YuanShen");
            return processes.Length > 0 ? processes[0] : null;
        }
    }
}
