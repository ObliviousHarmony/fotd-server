using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionModel
    {
        public short X;
        public short Y;
        public short Z;
    }
}
