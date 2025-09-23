using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Shared.Application.PacketHandlers
{
    public class ReadPacketErrorHandler : PacketHandler<ReadPacketError>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_FOM_PACKET_READ_ERROR;

        private readonly ILogService logService;

        public ReadPacketErrorHandler(ILogService logService)
        {
            this.logService = logService;
        }

        public override void Handle(NetworkAddress sender, in ReadPacketError data)
        {
            logService.Write(
                MessageLogEntry.Create(LogLevel.Error, $"Received read error from {sender}: Packet={data.OffendingID} Code={data.ErrorCode}")
            );
        }
    }
}
