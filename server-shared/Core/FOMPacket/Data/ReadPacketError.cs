using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReadPacketError
    {
        public enum ReadErrorCode : byte
        {
            ERROR_MISSING_PACKET_ID = 0,
            ERROR_UNHANDLED_PACKET_ID = 1,
            ERROR_DESERIALIZATION = 2
        }

        public PacketIdentifier OffendingID;
        public ReadErrorCode ErrorCode;
    }
}
