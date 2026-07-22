using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Core.Packets.Types.Item
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemList
    {
        public ushort ReservedSpace;
        public uint MaxSpace;
        public uint ItemCount;
        public ItemArray Items;

        [InlineArray(BufferSizes.MaxItemListSize)]
        public struct ItemArray
        {
            private Item _item;
        }
    }
}
