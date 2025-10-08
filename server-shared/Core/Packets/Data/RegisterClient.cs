using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_REGISTER_CLIENT)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RegisterClient
    {
        public WorldID WorldID;
        public uint PlayerID;
        public uint WorldCRC;
    }
}
