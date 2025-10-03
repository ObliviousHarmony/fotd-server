using FOMServer.Master.Application.FOMPacket;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class WorldOverviewHandler : PacketHandler<WorldOverview>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_WORLD_OVERVIEW;

        private readonly IPlayerService playerService;
        private readonly IWorldOverviewFactory worldOverviewFactory;
        private readonly IClientPacketSender packetSender;

        public WorldOverviewHandler(IPlayerService playerService, IWorldOverviewFactory worldOverviewFactory, IClientPacketSender packetSender)
        {
            this.playerService = playerService;
            this.worldOverviewFactory = worldOverviewFactory;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in WorldOverview data)
        {
            var player = playerService.Get(sender);
            if (player == null)
                return;
            if (player.ID != data.PlayerID)
                throw new InvalidOperationException($"Player {player.ID} Provided Wrong ID: {data.PlayerID}");

            var response = new WorldOverviewReturn()
            {
                PlayerID = player.ID,
                WorldOverview = this.worldOverviewFactory.Create(player),
            };

            packetSender.Send(
                PacketIdentifier.ID_WORLD_OVERVIEW_RETURN,
                new FOMDataUnion { worldOverviewReturn = response },
                sender,
                PacketPriority.HIGH_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
