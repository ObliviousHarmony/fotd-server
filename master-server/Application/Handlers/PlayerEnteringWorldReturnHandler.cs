using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Metadata;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class PlayerEnteringWorldReturnHandler : BasePacketHandler<PlayerEnteringWorldReturn>
    {
        private readonly IPlayerService _playerService;
        private readonly IWorldServerService _worldServerService;
        private readonly IClientPacketSender _packetSender;

        public PlayerEnteringWorldReturnHandler(
            IClientPacketSender packetSender,
            IWorldServerService worldServerService,
            IPlayerService playerService
        )
        {
            _packetSender = packetSender;
            _worldServerService = worldServerService;
            _playerService = playerService;
        }

        public override void Handle(NetworkAddress sender, in PlayerEnteringWorldReturn data)
        {
            var player = _playerService.Get(data.PlayerID);
            if (player == null)
                throw new InvalidOperationException($"Player not found for address {sender}");

            var worldServer = _worldServerService.Get(sender);
            if (worldServer == null)
                throw new InvalidOperationException($"World server not found for address {sender}");

            var response = new WorldLoginReturn()
            {
                WorldID = worldServer.ID,
            };

            if (data.Status == PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_READY)
                response.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SUCCESS;
            else if (data.Status == PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_SERVER_FULL)
                response.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SERVER_FULL;
            else
                response.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_INVALID;

            _packetSender.Send(
                response,
                player.ClientAddress,
                PacketPriority.MEDIUM_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
