using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Core.Dtos
{
    public record PlayerDto
    {
        public uint id { get; init; }

        public string name { get; init; } = "";

        public string biography { get; init; } = "";

        public AvatarConstants.Sex sex { get; init; }

        public AvatarConstants.Race race { get; init; }

        public ushort face { get; init; }

        public ushort hair { get; init; }

        public DateTime created_at { get; init; }

        public DateTime updated_at { get; init; }
    }
}
