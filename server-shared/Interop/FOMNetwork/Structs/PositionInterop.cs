using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionInterop
    {
        public short X;
        public short Y;
        public short Z;
    }
}
