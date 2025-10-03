using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Logging;

namespace FOMServer.Shared.Core.Handlers
{
    public class ReadPacketErrorHandler : PacketHandler<ReadPacketError>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_FOM_PACKET_READ_ERROR;

        private readonly ILogService _logService;

        public ReadPacketErrorHandler(ILogService logService)
        {
            _logService = logService;
        }

        public override void Handle(NetworkAddress sender, in ReadPacketError data)
        {
            _logService.Write(
                MessageLogEntry.Create(LogLevel.Error, $"Received read error from {sender}: Packet={data.OffendingID} Code={data.ErrorCode}")
            );
        }
    }
}
