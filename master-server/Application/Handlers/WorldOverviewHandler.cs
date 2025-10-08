using FOMServer.Master.Application.Packets;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
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

        public override void Handle(NetworkAddress sender, in WorldOverview p)
        {
            var player = _playerService.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");
            if (player.ID != p.PlayerID)
                throw new InvalidOperationException($"Player {player.ID} Provided Wrong ID: {p.PlayerID}");

            using var response = QueuePacket.Create<WorldOverviewReturn>();
            ref var rData = ref response.Data;

            rData.PlayerID = player.ID;
            rData.WorldOverview = _worldOverviewFactory.Create(player);

            _packetSender.Send(response, sender, PacketPriority.Medium, PacketReliability.ReliableOrdered);
        }
    }
}
