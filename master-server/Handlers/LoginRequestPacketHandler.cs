using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Packets;
using FOMServer.Shared.Services;

namespace FOMServer.Master.Handlers
{
	public class LoginRequestPacketHandler : PacketHandler<LoginRequest>
	{
		public LoginRequestPacketHandler(ILogService logService) : base(logService) { }

		public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN_REQUEST;

		public override void Handle(NetworkAddress sender, in LoginRequest data)
		{
			logService.WriteMessage(LogLevel.Info, $"Login Request: {data.Username} from {sender}");

		}
	}
}
