using System.Net.NetworkInformation;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class PlayerMigrateWorldHandler : PacketHandlerBase<PlayerMigrateWorld>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IWorldPacketSender _worldPacketSender;
        private readonly IClientRegistry _clientRegistry;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogger<PlayerMigrateWorldHandler> _logger;

        public PlayerMigrateWorldHandler(
            IClientPacketSender clientPacketSender,
            IWorldPacketSender worldPacketSender,
            IClientRegistry clientRegistry,
            IWorldServerRegistry worldServerRegistry,
            ILogger<PlayerMigrateWorldHandler> logger)
        {
            _clientPacketSender = clientPacketSender;
            _worldPacketSender = worldPacketSender;
            _clientRegistry = clientRegistry;
            _worldServerRegistry = worldServerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerMigrateWorld p)
        {
            var session = _clientRegistry.Get(p.PlayerId);
            if (session is null)
            {
                _logger.LogWarning("Received world migration for unknown player {PlayerId}", p.PlayerId);
                return;
            }

            if (!session.PendingWorld.HasValue)
            {
                _logger.LogWarning("Received world migration for player {PlayerId} with no transfer in progress", p.PlayerId);
                return;
            }

            var destination = session.PendingWorld.Value;
            var destinationServer = _worldServerRegistry.Get(destination);
            if (destinationServer is null)
            {
                session.AbortWorldTransfer();

                using var response = new PacketWriter<WorldLoginReturn>(session.Address);
                ref var rData = ref response.Data;
                rData.WorldId = session.PendingWorld.Value;
                rData.Status = WorldLoginReturn.StatusCode.ServerOffline;
                _clientPacketSender.Send(response.Build());
                return;
            }

            using var forward = new PacketWriter<PlayerMigrateWorld>(destinationServer.ServerAddress);
            forward.Data = p;
            _worldPacketSender.Send(forward.Build());
        }
    }
}
