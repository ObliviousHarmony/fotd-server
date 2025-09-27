using FOMServer.Master.Application.Services;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class LoginHandler : PacketHandler<Login>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN;

        private readonly ILogService logService;
        private readonly IAccountService accountService;
        private readonly IPacketSender packetSender;

        public LoginHandler(ILogService logService, IAccountService accountService, IPacketSender packetSender)
        {
            this.logService = logService;
            this.accountService = accountService;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login data)
        {
            var account = accountService.Login(data.Username, data.PasswordHash, sender);
            if (account == null)
                return;

            logService.WriteMessage(LogLevel.Info, $"Received login for {account.Username} ({account.ID}) - {data.MACAddress} from {sender}");

            var response = new LoginReturn()
            {
                Status = LoginReturn.StatusCode.LOGIN_RETURN_CREATE_CHARACTER,
            };

            packetSender.Send(
                PacketIdentifier.ID_LOGIN_RETURN,
                new FOMDataUnion { loginReturn = response },
                sender,
                PacketPriority.HIGH_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
