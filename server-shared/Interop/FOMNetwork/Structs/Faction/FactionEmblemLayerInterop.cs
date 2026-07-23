using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Faction
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionEmblemLayerInterop
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
