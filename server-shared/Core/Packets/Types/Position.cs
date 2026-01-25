using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Position
    {
        public short X;
        public short Y;
        public short Z;
        public ushort Rotation;
    }
}
