using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class LoginHandler : PacketHandler<Login>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN;

        private readonly IPlayerService playerService;
        private readonly IClientPacketSender packetSender;

        public LoginHandler(IPlayerService playerService, IClientPacketSender packetSender)
        {
            this.playerService = playerService;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login data)
        {
            var player = playerService.Login(data.Username, data.PasswordHash, sender);
            if (player == null)
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
