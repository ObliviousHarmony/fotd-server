using FOMServer.Master.Application.FOMPacket;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
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
        private readonly IWorldOverviewFactory worldOverviewFactory;
        private readonly IClientPacketSender packetSender;

        public LoginHandler(IPlayerService playerService, IWorldOverviewFactory worldOverviewFactory, IClientPacketSender packetSender)
        {
            this.playerService = playerService;
            this.worldOverviewFactory = worldOverviewFactory;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login data)
        {
            var player = playerService.Login(data.Username, data.PasswordHash, sender);
            if (player == null)
                return;

            LoginReturn response;
            if (player.HasCharacter)
            {
                response = new LoginReturn()
                {
                    Status = LoginReturn.StatusCode.LOGIN_RETURN_SUCCESS,
                    PlayerID = player.ID,
                    AccountType = 3,
                    IsVolunteer = false,
                    ClientVersion = GlobalConstants.ClientVersion,
                    WorldOverview = this.worldOverviewFactory.Create(player),
                };
            }
            else
            {
                response = new LoginReturn()
                {
                    Status = LoginReturn.StatusCode.LOGIN_RETURN_CREATE_CHARACTER,
                };
            }

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
