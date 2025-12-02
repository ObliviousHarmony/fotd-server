using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_FOM_PACKET_READ_ERROR)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReadPacketError
    {
        public PacketIdentifier OffendingID;
        public ReadErrorCode ErrorCode;

        public enum ReadErrorCode : byte
        {
            ERROR_UNHANDLED_PACKET_ID = 0,
            ERROR_DESERIALIZATION = 1
        }
    }
}
