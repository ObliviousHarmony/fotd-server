using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerEnteringWorld
    {
        public uint PlayerID;
        public byte NodeID;
    }
}
