using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerAttributes
    {
        public fixed uint Values[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
    }
}
