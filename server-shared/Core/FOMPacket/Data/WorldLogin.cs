using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldLogin
    {
        public WorldID WorldID;
        public byte NodeID;
        public uint PlayerID;
        public uint ApartmentID;
    }
}
