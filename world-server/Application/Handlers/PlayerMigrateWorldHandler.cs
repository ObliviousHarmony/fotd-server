using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class PlayerMigrateWorldHandler : PacketHandlerBase<PlayerMigrateWorld>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IMasterPacketSender _packetSender;
        private readonly ILogger<PlayerMigrateWorldHandler> _logger;

        public PlayerMigrateWorldHandler(
            IPlayerRegistry playerRegistry,
            IMasterPacketSender packetSender,
            ILogger<PlayerMigrateWorldHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _packetSender = packetSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerMigrateWorld p)
        {
            _ = _playerRegistry.PrepareForClient(p.PlayerID, p.ClientBinaryAddress);
            _logger.LogInformation("Prepared player {PlayerID} for takeover", p.PlayerID);

            using var response = new PacketWriter<PlayerWorldReady>();
            ref var rData = ref response.Data;

            rData.PlayerID = p.PlayerID;
            rData.Status = PlayerWorldReady.StatusCode.Success;

            _packetSender.Send(response.Build());
        }
    }
}
