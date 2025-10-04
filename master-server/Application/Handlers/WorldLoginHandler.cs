using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class WorldLoginHandler : PacketHandler<WorldLogin>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_WORLD_LOGIN;

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

        public override void Handle(NetworkAddress sender, in WorldLogin data)
        {
            var worldServer = _worldServerService.Get(data.WorldID);
            if (worldServer == null)
            {
                var response = new WorldLoginReturn()
                {
                    Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SERVER_UNAVAILABLE,
                    WorldID = data.WorldID
                };

                _clientPacketSender.Send(
                    response,
                    sender,
                    PacketPriority.MEDIUM_PRIORITY,
                    PacketReliability.RELIABLE_ORDERED
                );
                return;
            }

            var player = _playerService.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");

            if (player.ID != data.PlayerID)
                throw new InvalidOperationException($"Player {player.ID} Provided Wrong ID: {data.PlayerID}");

            var worldResponse = new PlayerEnteringWorld()
            {
                PlayerID = data.PlayerID,
                SelectedNodeID = data.SelectedNodeID,
            };

            _worldPacketSender.Send(
                worldResponse,
                worldServer.ServerAddress,
                PacketPriority.MEDIUM_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
