using FOMServer.Master.Application.Services;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class LoginRequestHandler : PacketHandler<LoginRequest>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN_REQUEST;

        private readonly IAccountRepository accountRepository;
        private readonly IAccountService accountService;
        private readonly IPacketSender packetSender;
        private readonly ILogService logService;

        public LoginRequestHandler(
            IAccountRepository accountRepository,
            IAccountService accountService,
            IPacketSender packetSender,
            ILogService logService
        )
        {
            this.accountService = accountService;
            this.accountRepository = accountRepository;
            this.packetSender = packetSender;
            this.logService = logService;
        }

        public override void Handle(NetworkAddress sender, in LoginRequest data)
        {
            var response = new LoginRequestReturn();
            unsafe
            {
                // We send back the username regardless of the outcome.
                for (int i = 0; i < 19; i++)
                    response.RawUsername[i] = data.RawUsername[i];
            }

            logService.WriteMessage(LogLevel.Debug, $"Login Request: {data.Username} from {sender}");

            var accountID = accountRepository.AccountExists(data.Username);
            if (accountID == null)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_INVALID_INFORMATION;
            else if (accountService.Get(accountID.Value) != null)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_ALREADY_LOGGED_IN;
            else if (data.ClientVersion != GlobalConstants.CLIENT_VERSION)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_OUTDATED_CLIENT;
            else
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_SUCCESS;

            packetSender.Send(
                PacketIdentifier.ID_LOGIN_REQUEST_RETURN,
                new FOMDataUnion { loginRequestReturn = response },
                sender,
                PacketPriority.HIGH_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
