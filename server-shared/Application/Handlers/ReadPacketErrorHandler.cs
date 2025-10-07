using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Handlers
{
    [PacketHandler]
    public class ReadPacketErrorHandler : BasePacketHandler<ReadPacketError>
    {
        private readonly ILogService _logService;

        public ReadPacketErrorHandler(ILogService logService)
        {
            _logService = logService;
        }

        public override void Handle(NetworkAddress sender, in ReadPacketError p)
        {
            _logService.Write(
                MessageLogEntry.Create(
                    LogLevel.Error,
                    $"Received read error from {sender}: Packet={p.OffendingID} Code={p.ErrorCode}"
                )
            );
        }
    }
}
