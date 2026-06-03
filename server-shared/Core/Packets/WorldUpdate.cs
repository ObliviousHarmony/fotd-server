using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_WORLD_UPDATE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldUpdate
    {
        public const int MaxWorldUpdates = 100; // MAX_WORLD_UPDATES

        public uint PlayerId;
        public uint Unknown1;
        public ushort UpdateCount;
        public UpdatesArray Updates;

        [InlineArray(MaxWorldUpdates)]
        public struct UpdatesArray
        {
            private Types.WorldUpdate _element;
        }
    }
}
