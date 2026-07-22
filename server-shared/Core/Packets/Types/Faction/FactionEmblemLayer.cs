using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types.Faction
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionEmblemLayer
    {
        public ushort Shape;
        public sbyte OffsetX;
        public sbyte OffsetY;
        public byte ScaleWidth;
        public byte ScaleHeight;
        public ushort Rotation;
        public byte Red;
        public byte Green;
        public byte Blue;
    }
}
