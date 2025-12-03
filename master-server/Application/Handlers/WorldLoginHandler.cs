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
    public class WorldLoginHandler : PacketHandlerBase<WorldLogin>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IWorldPacketSender _worldPacketSender;

        public WorldLoginHandler(
            IPlayerRegistry playerRegistry,
            IWorldServerRegistry worldServerRegistry,
            IClientPacketSender clientPacketSender,
            IWorldPacketSender worldPacketSender
        )
        {
            _playerRegistry = playerRegistry;
            _worldServerRegistry = worldServerRegistry;
            _clientPacketSender = clientPacketSender;
            _worldPacketSender = worldPacketSender;
        }

        public override void Handle(NetworkAddress sender, in WorldLogin p)
        {
            var worldServer = _worldServerRegistry.Get(p.WorldID);
            if (worldServer == null)
            {
                using var unavailableResponse = new PacketWriter<WorldLoginReturn>();
                ref var urData = ref unavailableResponse.Data;

                urData.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SERVER_UNAVAILABLE;
                urData.WorldID = p.WorldID;

                unavailableResponse.AddDestination(sender);
                _clientPacketSender.Send(unavailableResponse.Build());
                return;
            }

            var player = _playerRegistry.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");

            if (player.ID != p.PlayerID)
                throw new InvalidOperationException($"Player {player.ID} Provided Wrong ID: {p.PlayerID}");

            using var worldResponse = new PacketWriter<PlayerEnteringWorld>();
            ref var wrData = ref worldResponse.Data;

            wrData.PlayerID = p.PlayerID;
            wrData.SelectedNodeID = p.SelectedNodeID;

            worldResponse.AddDestination(worldServer.ServerAddress);
            _worldPacketSender.Send(worldResponse.Build());
        }
    }
}
