using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Packets;
using FOMServer.Shared.Services;

namespace FOMServer.Master.Handlers
{
	public class LoginRequestHandler : PacketHandler<LoginRequest>
	{
		public LoginRequestHandler(ILogService logService) : base(logService) { }

		public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN_REQUEST;

		public override void Handle(NetworkAddress sender, in LoginRequest data)
		{
			logService.WriteMessage(LogLevel.Info, $"Login Request: {data.Username} from {sender}");

		}
	}
}
