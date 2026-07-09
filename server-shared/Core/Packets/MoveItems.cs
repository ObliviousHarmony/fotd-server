using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_MOVE_ITEMS)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MoveItems
    {
        public uint PlayerId;
        public ushort NumIds;
        public fixed uint Ids[BufferSizes.MaxItemListSize];
        public ItemLocation Source;
        public ItemLocation Destination;
        public ItemSlot SourceSlot;
        public ItemSlot DestinationSlot;
    }
}
