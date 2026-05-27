using System.Runtime.InteropServices;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(Enums.PacketIdentifier.ID_REGISTER_CLIENT)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterClient
    {
        public byte WorldId;
        public uint PlayerId;
        public uint WorldCrc;
    }
}
