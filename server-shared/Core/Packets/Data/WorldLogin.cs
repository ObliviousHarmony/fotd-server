using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_WORLD_LOGIN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldLogin
    {
        public WorldID WorldID;
        public byte SelectedNodeID;
        public uint PlayerID;
        public uint ApartmentID;
    }
}
