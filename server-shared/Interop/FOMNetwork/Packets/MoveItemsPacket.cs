using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_MOVE_ITEMS)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MoveItemsPacket
    {
        public uint PlayerId;
        public ushort NumItemIds;
        public fixed uint RawItemIds[BufferSizes.MaxItemListSize];
        public ItemContainerType From;
        public ItemContainerType To;
        public ItemSlotType FromSlot;
        public ItemSlotType ToSlot;

        public ReadOnlySpan<uint> ItemIds
        {
            get
            {
                fixed (uint* ptr = RawItemIds)
                {
                    return new Span<uint>(ptr, NumItemIds);
                }
            }
        }
    }
}
