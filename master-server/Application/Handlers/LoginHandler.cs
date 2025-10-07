using FOMServer.Master.Application.Packets;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
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

        public override void Handle(NetworkAddress sender, in Login p)
        {
            var player = _playerService.Login(p.Username, p.PasswordHash, sender);
            if (player == null)
                return;

            using var response = QueuePacket.Create<LoginReturn>();
            ref var rData = ref response.Data;

            if (player.HasCharacter)
            {
                rData.Status = LoginReturn.StatusCode.LOGIN_RETURN_SUCCESS;
                rData.PlayerID = player.ID;
                rData.AccountType = 3;
                rData.IsVolunteer = false;
                rData.ClientVersion = GlobalConstants.ClientVersion;
                rData.WorldOverview = _worldOverviewFactory.Create(player);
            }
            else
                rData.Status = LoginReturn.StatusCode.LOGIN_RETURN_CREATE_CHARACTER;

            _packetSender.Send(response, sender, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
