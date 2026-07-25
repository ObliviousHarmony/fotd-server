using FOMServer.Shared.Interop.FOMNetwork.Constants;

namespace FOMServer.Shared.Core.Dtos
{
    public record PlayerDto
    {
        public uint id { get; init; }

        public string name { get; init; } = "";

        public AvatarConstants.Sex sex { get; init; }

        public AvatarConstants.Race race { get; init; }

        public ushort face { get; init; }

        public ushort hair { get; init; }
    }
}
