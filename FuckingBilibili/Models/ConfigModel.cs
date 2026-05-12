namespace FuckingBilibili.Models
{
    public enum ServerType
    {
        Official,
        Bilibili
    }

    public class ConfigModel
    {
        public int SubChannel { get; set; }
        public string GameBiz { get; set; } = string.Empty;
        public int Channel { get; set; }

        public ServerType GetServerType()
        {
            if (GameBiz == "hk4e_bilibili" || Channel == 14)
                return ServerType.Bilibili;
            return ServerType.Official;
        }

        public string GetServerName()
        {
            return GetServerType() == ServerType.Official ? "官服" : "B服";
        }
    }
}
