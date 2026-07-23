using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.PacketHandlers
{
    [PacketHandler]
    internal class PlayerLeavingWorldPacketHandler : PacketHandlerBase<PlayerLeavingWorldPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IMasterPacketSender _masterPacketSender;
        private readonly ILogger<PlayerLeavingWorldPacketHandler> _logger;

        public PlayerLeavingWorldPacketHandler(
            IPlayerRegistry playerRegistry,
            IMasterPacketSender masterPacketSender,
            ILogger<PlayerLeavingWorldPacketHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _masterPacketSender = masterPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerLeavingWorldPacket p)
        {
            var player = _playerRegistry.Get(p.PlayerId);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected player migration request for player {PlayerId}", p.PlayerId);
                return;
            }

            using var migrate = new PacketWriter<PlayerMigrateWorldPacket>(sender);
            ref var mData = ref migrate.Data;

            mData.PlayerId = p.PlayerId;
            mData.ClientBinaryAddress = player.Address.BinaryAddress;

            _masterPacketSender.Send(migrate.Build());
            _playerRegistry.Logout(player);
        }
    }
}
