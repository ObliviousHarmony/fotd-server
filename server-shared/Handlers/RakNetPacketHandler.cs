using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Services;

namespace FOMServer.Shared.Handlers
{
	public class RakNetPacketHandler
	{
		private readonly ILogService logService;

		public RakNetPacketHandler(ILogService logService)
		{
			this.logService = logService;
		}

		public void Handle(PacketIdentifier id, NetworkAddress sender)
		{
			if (id != PacketIdentifier.ID_NEW_INCOMING_CONNECTION)
				return;

			logService.WriteMessage(LogLevel.Debug, $"New incoming connection from {sender}");
		}
	}
}
