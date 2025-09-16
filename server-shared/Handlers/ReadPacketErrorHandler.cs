using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Packets;
using FOMServer.Shared.Services;
using FOMServer.Shared.Services.Packets;

namespace FOMServer.Shared.Handlers
{
	public class ReadPacketErrorHandler : PacketHandler<ReadPacketError>
	{
		public ReadPacketErrorHandler(ILogService logService) : base(logService) { }

		public override PacketIdentifier PacketID => PacketIdentifier.ID_FOM_PACKET_READ_ERROR;

		public override void Handle(NetworkAddress sender, in ReadPacketError data)
		{
			logService.Write(
				MessageLogEntry.Create(Enums.LogLevel.Error, $"Received read error from {sender}: Packet={data.OffendingID} Code={data.ErrorCode}")
			);
		}
	}
}
