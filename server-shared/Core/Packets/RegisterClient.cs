using System.Runtime.InteropServices;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(Enums.PacketIdentifier.ID_REGISTER_CLIENT)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterClient
    {
        public byte WorldID;
        public uint PlayerID;
        public uint WorldCRC;
    }
}
