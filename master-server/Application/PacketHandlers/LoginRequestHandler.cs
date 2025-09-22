using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
{
	public class LoginRequestHandler : PacketHandler<LoginRequest>
	{
		private readonly IPacketSender sendPacketService;

		public LoginRequestHandler(
			ILogService logService,
			IPacketSender sendPacketService
			)
			: base(logService)
		{
			this.sendPacketService = sendPacketService;
		}

		public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN_REQUEST;

		public override void Handle(NetworkAddress sender, in LoginRequest data)
		{
			logService.WriteMessage(LogLevel.Info, $"Login Request: {data.Username} from {sender}");

			var response = new LoginRequestReturn
			{
				Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_SUCCESS
			};

			unsafe
			{
				for (int i = 0; i < 19; i++)
					response.Username[i] = data.username[i];
			}

			sendPacketService.Send(
				PacketIdentifier.ID_LOGIN_REQUEST_RETURN,
				new FOMDataUnion{ loginRequestReturn = response },
				sender,
				PacketPriority.HIGH_PRIORITY,
				PacketReliability.RELIABLE_ORDERED
			);
		}
	}
}
