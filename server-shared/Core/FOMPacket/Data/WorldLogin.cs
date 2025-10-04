using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_WORLD_LOGIN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldLogin
    {
        public WorldID WorldID;
        public byte NodeID;
        public uint PlayerID;
        public uint ApartmentID;
    }
}
