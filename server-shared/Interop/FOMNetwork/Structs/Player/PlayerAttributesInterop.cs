using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Player
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerAttributesInterop
    {
        public fixed uint Values[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
    }
}
