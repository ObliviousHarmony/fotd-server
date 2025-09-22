using FOMServer.Master.Application.Services;
using FOMServer.Master.Core.Interfaces;
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
		private readonly IAccountRepository accountRepository;
		private readonly IAccountService accountService;
		private readonly IPacketSender sendPacketService;
		private readonly ILogService logService;

		public LoginRequestHandler(
			IAccountRepository accountRepository,
			IAccountService accountService,
			IPacketSender sendPacketService,
			ILogService logService
		)
		{
			this.accountService = accountService;
			this.accountRepository = accountRepository;
			this.sendPacketService = sendPacketService;
			this.logService = logService;
		}

		public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN_REQUEST;

		public override void Handle(NetworkAddress sender, in LoginRequest data)
		{
			string username;
			var response = new LoginRequestReturn();
			unsafe
			{
				fixed (byte* ptr = data.Username)
					username = CStringParser.ToString(ptr, 19);
				// We send back the username regardless of the outcome.
				for (int i = 0; i < 19; i++)
					response.Username[i] = data.Username[i];
			}

			logService.WriteMessage(LogLevel.Info, $"Login Request: {username} from {sender}");

			var accountID = accountRepository.AccountExists(username);
			if (accountID == null)
				response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_INVALID_INFORMATION;
			else if (accountService.Get(accountID.Value) != null)
				response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_ALREADY_LOGGED_IN;
			else
				response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_SUCCESS;

			sendPacketService.Send(
				PacketIdentifier.ID_LOGIN_REQUEST_RETURN,
				new FOMDataUnion { loginRequestReturn = response },
				sender,
				PacketPriority.HIGH_PRIORITY,
				PacketReliability.RELIABLE_ORDERED
			);
		}
	}
}
