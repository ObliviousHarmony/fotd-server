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
    internal class PlayerLeavingWorldHandler : PacketHandlerBase<PlayerLeavingWorld>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IMasterPacketSender _masterPacketSender;
        private readonly ILogger<PlayerLeavingWorldHandler> _logger;

        public PlayerLeavingWorldHandler(
            IPlayerRegistry playerRegistry,
            IMasterPacketSender masterPacketSender,
            ILogger<PlayerLeavingWorldHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _masterPacketSender = masterPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerLeavingWorld p)
        {
            var player = _playerRegistry.Get(p.PlayerId);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected player migration request for player {PlayerId}", p.PlayerId);
                return;
            }

            using var migrate = new PacketWriter<PlayerMigrateWorld>(sender);
            ref var mData = ref migrate.Data;

            mData.PlayerId = p.PlayerId;
            mData.ClientBinaryAddress = player.Address.BinaryAddress;

            _masterPacketSender.Send(migrate.Build());
            _playerRegistry.Logout(player);
        }
    }
}
