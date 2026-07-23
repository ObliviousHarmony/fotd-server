using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class PlayerMigrateWorldPacketHandler : PacketHandlerBase<PlayerMigrateWorldPacket>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IWorldPacketSender _worldPacketSender;
        private readonly IClientRegistry _clientRegistry;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogger<PlayerMigrateWorldPacketHandler> _logger;

        public PlayerMigrateWorldPacketHandler(
            IClientPacketSender clientPacketSender,
            IWorldPacketSender worldPacketSender,
            IClientRegistry clientRegistry,
            IWorldServerRegistry worldServerRegistry,
            ILogger<PlayerMigrateWorldPacketHandler> logger
        )
        {
            _clientPacketSender = clientPacketSender;
            _worldPacketSender = worldPacketSender;
            _clientRegistry = clientRegistry;
            _worldServerRegistry = worldServerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerMigrateWorldPacket p)
        {
            var session = _clientRegistry.Get(p.PlayerId);
            if (session is null)
            {
                _logger.LogWarning("Received world migration for unknown player {PlayerId}", p.PlayerId);
                return;
            }

            if (!session.PendingWorld.HasValue)
            {
                _logger.LogWarning("Received unexpected world migration for player {PlayerId}", p.PlayerId);
                return;
            }

            var destination = session.PendingWorld.Value;
            var destinationServer = _worldServerRegistry.Get(destination);
            if (destinationServer is null)
            {
                session.AbortWorldTransfer();

                using var response = new PacketWriter<WorldLoginReturnPacket>(session.Address);
                ref var rData = ref response.Data;
                rData.WorldId = session.PendingWorld.Value;
                rData.Status = WorldLoginReturnPacket.StatusCode.ServerOffline;
                _clientPacketSender.Send(response.Build());
                return;
            }

            using var forward = new PacketWriter<PlayerMigrateWorldPacket>(destinationServer.ServerAddress);
            forward.Data = p;
            _worldPacketSender.Send(forward.Build());
        }
    }
}
