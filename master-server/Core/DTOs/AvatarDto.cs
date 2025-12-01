using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Core.DTOs
{
    public class AvatarDto
    {
        public uint player_id { get; init; }
        public string name { get; init; } = "";
        public Faction faction { get; init; }
        public AvatarSex sex { get; init; }
        public AvatarSkin skin_color { get; init; }
        public byte face { get; init; }
        public byte hair { get; init; }
    }
}
