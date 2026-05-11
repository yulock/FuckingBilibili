namespace FuckingBilibili.Models
{
    /// <summary>
    /// 原神服务器类型
    /// </summary>
    public enum ServerType
    {
        Official,   // 官服
        Bilibili    // B服
    }

    /// <summary>
    /// config.ini 配置模型
    /// </summary>
    public class ConfigModel
    {
        /// <summary>
        /// 子频道号 官服=1 B服=0
        /// </summary>
        public int SubChannel { get; set; }

        /// <summary>
        /// 游戏业务标识 官服=hk4e_cn B服=hk4e_bilibili
        /// </summary>
        public string GameBiz { get; set; } = string.Empty;

        /// <summary>
        /// 渠道号 官服=1 B服=14
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// 根据配置判断当前服务器类型
        /// </summary>
        public ServerType GetServerType()
        {
            if (GameBiz == "hk4e_bilibili" || Channel == 14)
                return ServerType.Bilibili;
            return ServerType.Official;
        }

        /// <summary>
        /// 获取服务器显示名称
        /// </summary>
        public string GetServerName()
        {
            return GetServerType() == ServerType.Official ? "官服" : "B服";
        }
    }
}
