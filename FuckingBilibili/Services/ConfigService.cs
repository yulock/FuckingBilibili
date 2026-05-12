using System;
using System.IO;
using System.Linq;
using FuckingBilibili.Models;

namespace FuckingBilibili.Services
{
    public class ConfigService
    {
        private const string ConfigFileName = "config.ini";

        public ConfigModel? ReadConfig(string gamePath)
        {
            try
            {
                string configPath = Path.Combine(gamePath, ConfigFileName);
                if (!File.Exists(configPath))
                    return null;

                var config = new ConfigModel();
                var lines = File.ReadAllLines(configPath);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                        continue;

                    var parts = trimmedLine.Split(new[] { '=' }, 2);
                    if (parts.Length != 2)
                        continue;

                    var key = parts[0].Trim().ToLower();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "sub_channel":
                            if (int.TryParse(value, out int subChannel))
                                config.SubChannel = subChannel;
                            break;
                        case "channel":
                            if (int.TryParse(value, out int channel))
                                config.Channel = channel;
                            break;
                        case "game_biz":
                            config.GameBiz = value;
                            break;
                    }
                }

                return config;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取配置文件失败: {ex.Message}");
            }
        }

        public void WriteConfig(string gamePath, ServerType serverType)
        {
            try
            {
                string configPath = Path.Combine(gamePath, ConfigFileName);
                if (!File.Exists(configPath))
                    throw new FileNotFoundException("配置文件不存在");

                var lines = File.ReadAllLines(configPath).ToList();

                int subChannel = serverType == ServerType.Bilibili ? 0 : 1;
                string gameBiz = serverType == ServerType.Bilibili ? "hk4e_bilibili" : "hk4e_cn";
                int channel = serverType == ServerType.Bilibili ? 14 : 1;

                UpdateOrAddConfigLine(lines, "sub_channel", subChannel.ToString());
                UpdateOrAddConfigLine(lines, "game_biz", gameBiz);
                UpdateOrAddConfigLine(lines, "channel", channel.ToString());

                File.WriteAllLines(configPath, lines);
            }
            catch (Exception ex)
            {
                throw new Exception($"写入配置文件失败: {ex.Message}");
            }
        }

        private void UpdateOrAddConfigLine(System.Collections.Generic.List<string> lines, string key, string value)
        {
            bool found = false;
            string keyLower = key.ToLower();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith(";"))
                    continue;

                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length >= 1 && parts[0].Trim().ToLower() == keyLower)
                {
                    lines[i] = $"{key}={value}";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                lines.Add($"{key}={value}");
            }
        }
    }
}
