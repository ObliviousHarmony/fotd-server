using FOMServer.Master.Application.FOMPacket;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class LoginHandler : BasePacketHandler<Login>
    {
        private readonly IPlayerService _playerService;
        private readonly IWorldOverviewFactory _worldOverviewFactory;
        private readonly IClientPacketSender _packetSender;

        public LoginHandler(IPlayerService playerService, IWorldOverviewFactory worldOverviewFactory, IClientPacketSender packetSender)
        {
            _playerService = playerService;
            _worldOverviewFactory = worldOverviewFactory;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login data)
        {
            var player = _playerService.Login(data.Username, data.PasswordHash, sender);
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
                    WorldOverview = _worldOverviewFactory.Create(player),
                };
            }
            else
            {
                response = new LoginReturn()
                {
                    Status = LoginReturn.StatusCode.LOGIN_RETURN_CREATE_CHARACTER,
                };
            }

            _packetSender.Send(response, sender, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
