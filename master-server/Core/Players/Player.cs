using FOMServer.Shared.Core.Players;

namespace FOMServer.Master.Core.Players
{
    public class Player : PlayerBase
    {
        public string Username { get; init; } = "";
        public bool HasAvatar { get; set; }
    }
}
