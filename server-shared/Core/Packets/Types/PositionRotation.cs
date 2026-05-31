using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionRotation
    {
        public Position Pos;
        public ushort Rot;
    }
}
