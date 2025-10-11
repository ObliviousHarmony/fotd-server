using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemSlotModel
    {
        public byte RawInUse;
        public ItemModel Item; // RawInUse == 1

        public bool InUse
        {
            get { return RawInUse != 0; }
            set { RawInUse = (byte)(value ? 1 : 0); }
        }
    }
}
