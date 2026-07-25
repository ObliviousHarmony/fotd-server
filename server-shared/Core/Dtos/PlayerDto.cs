using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

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

        public ItemType quickslot_1 { get; init; }

        public ItemType quickslot_2 { get; init; }

        public ItemType quickslot_3 { get; init; }

        public ItemType quickslot_4 { get; init; }
    }
}
