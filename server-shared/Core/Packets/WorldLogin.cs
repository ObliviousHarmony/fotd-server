using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_WORLD_LOGIN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldLogin
    {
        public WorldId WorldId;
        public byte NodeId;
        public uint PlayerId;
        public uint Constant;
    }
}
