using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_REGISTER_CLIENT)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterClientPacket
    {
        public byte WorldId;
        public uint PlayerId;
        public uint WorldCrc;
    }
}
