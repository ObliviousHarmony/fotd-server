using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldPlacementModel
    {
        public ushort X;
        public ushort Y;
        public ushort Z;
        public ushort Rotation;
    }
}
