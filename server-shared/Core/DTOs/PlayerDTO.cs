using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.DTOs
{
    public record PlayerDTO
    {
        public uint id { get; init; }
        public string name { get; init; } = "";
        public string biography { get; init; } = "";
        public AvatarSex sex { get; init; }
        public AvatarRace race { get; init; }
        public ushort face { get; init; }
        public ushort hair { get; init; }
        public DateTime created_at { get; init; }
        public DateTime updated_at { get; init; }

    }
}
