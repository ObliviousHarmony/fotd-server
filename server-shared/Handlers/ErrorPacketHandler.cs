using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Packets;
using FOMServer.Shared.Services;

namespace FOMServer.Shared.Handlers
{
	public class ErrorPacketHandler : PacketHandler<FOMPacketError>
	{
		public ErrorPacketHandler(ILogService logService) : base(logService) {}

		public override PacketIdentifier PacketID => PacketIdentifier.ID_FOM_PACKET_ERROR;

		/// <summary>
		/// Handles the error packet that was received.
		/// </summary>
		public override void Handle(NetworkAddress sender, FOMPacketError data)
		{
			logService.Write(
				MessageLogEntry.Create(Enums.LogLevel.Error, $"Received error packet from {sender}: Packet ID={data.OffendingID} Code={data.ErrorCode}")
			);
		}
	}
}
