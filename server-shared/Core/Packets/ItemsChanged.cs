using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    // Server -> client in-place update of existing items (matched by instance id).
    // Flat count + Item list (no dest, no stack grouping); Count is a byte so it
    // is capped at 255. Mirrors the native ItemsChanged.h.
    [PacketId(PacketIdentifier.ID_ITEMS_CHANGED)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemsChanged
    {
        public const int MaxItems = 255;

        public uint PlayerId;
        public byte Count;
        public ItemArray Items;

        [InlineArray(MaxItems)]
        public struct ItemArray
        {
            private Item _item;
        }
    }
}
