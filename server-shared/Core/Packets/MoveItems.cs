using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Enums.Item;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_MOVE_ITEMS)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MoveItems
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
