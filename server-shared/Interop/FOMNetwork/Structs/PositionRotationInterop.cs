using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionRotationInterop
    {
        public PositionInterop Pos;
        public ushort Rot;
    }
}
