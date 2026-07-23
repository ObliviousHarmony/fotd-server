using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Item
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemListInterop
    {
        public ushort ReservedSpace;
        public uint MaxSpace;
        public uint ItemCount;
        public ItemArray Items;

        [InlineArray(BufferSizes.MaxItemListSize)]
        public struct ItemArray
        {
            private ItemInterop _item;
        }
    }
}
