using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_WORLD_LOGIN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldLoginPacket
    {
        public WorldId WorldId;
        public byte NodeId;
        public uint PlayerId;
        public uint Constant;
    }
}
