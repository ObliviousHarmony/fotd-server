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
    public class WorldLoginHandler : BasePacketHandler<WorldLogin>
    {
        private readonly IPlayerService _playerService;
        private readonly IWorldServerService _worldServerService;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IWorldPacketSender _worldPacketSender;

        public WorldLoginHandler(
            IPlayerService playerService,
            IWorldServerService worldServerService,
            IClientPacketSender clientPacketSender,
            IWorldPacketSender worldPacketSender
        )
        {
            _playerService = playerService;
            _worldServerService = worldServerService;
            _clientPacketSender = clientPacketSender;
            _worldPacketSender = worldPacketSender;
        }

        public override void Handle(NetworkAddress sender, in WorldLogin p)
        {
            var worldServer = _worldServerService.Get(p.WorldID);
            if (worldServer == null)
            {
                using var unavailableResponse = new PacketBuilder<WorldLoginReturn>();
                ref var urData = ref unavailableResponse.Data;

                urData.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SERVER_UNAVAILABLE;
                urData.WorldID = p.WorldID;

                unavailableResponse.WithAddress(sender);
                _clientPacketSender.Send(unavailableResponse.Build());
                return;
            }

            var player = _playerService.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");

            if (player.ID != p.PlayerID)
                throw new InvalidOperationException($"Player {player.ID} Provided Wrong ID: {p.PlayerID}");

            using var worldResponse = new PacketBuilder<PlayerEnteringWorld>();
            ref var wrData = ref worldResponse.Data;

            wrData.PlayerID = p.PlayerID;
            wrData.SelectedNodeID = p.SelectedNodeID;

            worldResponse.WithAddress(worldServer.ServerAddress);
            _worldPacketSender.Send(worldResponse.Build());
        }
    }
}
