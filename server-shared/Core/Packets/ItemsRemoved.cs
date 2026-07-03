using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    // Server -> client removal of items (by instance id) from a container.
    // Mirrors the native ItemsRemoved.h.
    [PacketId(PacketIdentifier.ID_ITEMS_REMOVED)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemsRemoved
    {
        public const int MaxIds = 255;

        public uint PlayerId;
        public byte Dest;
        public ushort IdCount;
        public IdArray Ids;

        [InlineArray(MaxIds)]
        public struct IdArray
        {
            private uint _id;
        }
    }
}
