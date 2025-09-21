using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
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
