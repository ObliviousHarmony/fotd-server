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
        private readonly IClientPacketSender _packetSender;

        public WorldLoginHandler(
            IPlayerService playerService,
            IWorldServerService worldServerService,
            IClientPacketSender packetSender
        )
        {
            _playerService = playerService;
            _worldServerService = worldServerService;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in WorldLogin data)
        {
            var player = _playerService.Get(sender);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");

            var response = new WorldLoginReturn() { WorldID = data.WorldID };

            var worldServer = _worldServerService.Get(data.WorldID);
            if (worldServer == null)
                response.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SERVER_UNAVAILABLE;
            else
                response.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SUCCESS;

            _packetSender.Send(response, sender, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
