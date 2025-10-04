using FOMServer.Master.Application.FOMPacket;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class WorldOverviewHandler : BasePacketHandler<WorldOverview>
    {
        private readonly IPlayerService _playerService;
        private readonly IWorldOverviewFactory _worldOverviewFactory;
        private readonly IClientPacketSender _packetSender;

        public WorldOverviewHandler(
            IPlayerService playerService,
            IWorldOverviewFactory worldOverviewFactory,
            IClientPacketSender packetSender
        )
        {
            _playerService = playerService;
            _worldOverviewFactory = worldOverviewFactory;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in WorldOverview data)
        {
            var player = _playerService.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");
            if (player.ID != data.PlayerID)
                throw new InvalidOperationException($"Player {player.ID} Provided Wrong ID: {data.PlayerID}");

            var response = new WorldOverviewReturn()
            {
                PlayerID = player.ID,
                WorldOverview = _worldOverviewFactory.Create(player),
            };

            _packetSender.Send(response, sender, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
