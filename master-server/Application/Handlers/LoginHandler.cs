using FOMServer.Master.Core.Accounts;
using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class LoginHandler : PacketHandler<Login>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN;

        private readonly IAccountService accountService;
        private readonly IClientPacketSender packetSender;

        public LoginHandler(IAccountService accountService, IClientPacketSender packetSender)
        {
            this.accountService = accountService;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login data)
        {
            var account = accountService.Login(data.Username, data.PasswordHash, sender);
            if (account == null)
                return;

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
