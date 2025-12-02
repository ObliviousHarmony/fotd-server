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
    public class PlayerEnteringWorldReturnHandler : PacketHandlerBase<PlayerEnteringWorldReturn>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IWorldServerService _worldServerService;
        private readonly IClientPacketSender _packetSender;

        public PlayerEnteringWorldReturnHandler(
            IClientPacketSender packetSender,
            IWorldServerService worldServerService,
            IPlayerRegistry playerRegistry
        )
        {
            _packetSender = packetSender;
            _worldServerService = worldServerService;
            _playerRegistry = playerRegistry;
        }

        public override void Handle(NetworkAddress sender, in PlayerEnteringWorldReturn p)
        {
            var player = _playerRegistry.Get(p.PlayerID);
            if (player == null)
                throw new InvalidOperationException($"Player {p.PlayerID} not found");

            var worldServer = _worldServerService.Get(sender);
            if (worldServer == null)
                throw new InvalidOperationException($"World server not found for address {sender}");

            using var response = new PacketWriter<WorldLoginReturn>();
            ref var rData = ref response.Data;

            rData.WorldID = worldServer.ID;
            if (p.Status == PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_READY)
                rData.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SUCCESS;
            else if (p.Status == PlayerEnteringWorldReturn.StatusCode.PLAYER_ENTERING_WORLD_RETURN_SERVER_FULL)
                rData.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_SERVER_FULL;
            else
                rData.Status = WorldLoginReturn.StatusCode.WORLD_LOGIN_RETURN_INVALID;

            response.AddDestination(player.ClientAddress);
            _packetSender.Send(response.Build());
        }
    }
}
