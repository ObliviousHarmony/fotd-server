using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemModel
    {
        public uint ID;
        public ItemType Type;
        public ushort Value;
        public ushort Durability;
        public byte RawIsFactionItem;

        public bool IsFactionItem
        {
            get { return RawIsFactionItem != 0; }
            set { RawIsFactionItem = (byte)(value ? 1 : 0); }
        }
    }
}
