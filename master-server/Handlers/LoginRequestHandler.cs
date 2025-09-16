using System;
using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;
using FOMServer.Shared.Packets;
using FOMServer.Shared.Services;

namespace FOMServer.Master.Handlers
{
	public class LoginRequestHandler : PacketHandler<LoginRequest>
	{
		private readonly ISendPackets sendPacketService;

		public LoginRequestHandler(
			ILogService logService,
			ISendPackets sendPacketService)
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
				new FOMData{ loginRequestReturn = response },
				sender,
				PacketPriority.HIGH_PRIORITY,
				PacketReliability.RELIABLE_ORDERED
			);
		}
	}
}
