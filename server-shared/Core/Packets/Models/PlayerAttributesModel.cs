using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerAttributesModel
    {
        public fixed uint RawAttributes[(int)PlayerAttribute.NUM_ATTRIBUTES];

        public uint this[PlayerAttribute index]
        {
            get
            {
                if (index >= PlayerAttribute.NUM_ATTRIBUTES)
                    throw new IndexOutOfRangeException();

                fixed (uint* ptr = RawAttributes)
                {
                    return ptr[(int)index];
                }
            }
            set
            {
                if (index >= PlayerAttribute.NUM_ATTRIBUTES)
                    throw new IndexOutOfRangeException();

                fixed (uint* ptr = RawAttributes)
                {
                    ptr[(int)index] = value;
                }
            }
        }
    }
}
