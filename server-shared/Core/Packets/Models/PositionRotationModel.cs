using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionRotationModel
    {
        public PositionModel Position;
        public ushort Rotation;
    }
}
