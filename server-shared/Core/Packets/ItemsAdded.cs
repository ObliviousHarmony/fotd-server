using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_ITEMS_ADDED)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemsAdded
    {
        public uint PlayerId;
        public byte Dest;
        public ItemList Items;
    }
}
