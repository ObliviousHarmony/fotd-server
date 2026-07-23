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
    internal class PlayerMigrateWorldPacketHandler : PacketHandlerBase<PlayerMigrateWorldPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IMasterPacketSender _masterPacketSender;
        private readonly ILogger<PlayerMigrateWorldPacketHandler> _logger;

        public PlayerMigrateWorldPacketHandler(
            IPlayerRegistry playerRegistry,
            IMasterPacketSender masterPacketSender,
            ILogger<PlayerMigrateWorldPacketHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _masterPacketSender = masterPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerMigrateWorldPacket p)
        {
            _playerRegistry.PrepareForClient(p.PlayerId, p.ClientBinaryAddress);

            using var response = new PacketWriter<PlayerWorldReadyPacket>(sender);
            ref var rData = ref response.Data;

            rData.PlayerId = p.PlayerId;
            rData.Status = PlayerWorldReadyPacket.StatusCode.Success;

            _masterPacketSender.Send(response.Build());
        }
    }
}
