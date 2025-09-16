using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Services;
using FOMServer.Shared.Services.Packets;

namespace FOMServer.Master.Handlers
{
	public class IncomingConectionHandler : PacketHandler<RakNetPacket>
	{
		public IncomingConectionHandler(ILogService logService) : base(logService) { }

		public override PacketIdentifier PacketID => PacketIdentifier.ID_NEW_INCOMING_CONNECTION;

		public override void Handle(NetworkAddress sender, in RakNetPacket data)
		{
			logService.WriteMessage(LogLevel.Debug, $"New incoming connection from {sender}");
		}
	}
}
