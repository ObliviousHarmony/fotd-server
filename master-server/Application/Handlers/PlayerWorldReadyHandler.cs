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
    internal class PlayerWorldReadyHandler : PacketHandlerBase<PlayerWorldReady>
    {
        private readonly IClientPacketSender _packetSender;
        private readonly IClientRegistry _clientRegistry;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogger<PlayerWorldReadyHandler> _logger;

        public PlayerWorldReadyHandler(
            IClientPacketSender packetSender,
            IClientRegistry clientRegistry,
            IWorldServerRegistry worldServerRegistry,
            ILogger<PlayerWorldReadyHandler> logger)
        {
            _packetSender = packetSender;
            _clientRegistry = clientRegistry;
            _worldServerRegistry = worldServerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerWorldReady p)
        {
            var session = _clientRegistry.Get(p.PlayerId);
            if (session is null)
            {
                _logger.LogWarning("Received world-ready for unknown player {PlayerId}", p.PlayerId);
                return;
            }

            if (!session.PendingWorld.HasValue)
            {
                _logger.LogWarning("Received world-ready for player {PlayerId} with no transfer in progress", p.PlayerId);
                return;
            }

            var worldId = session.PendingWorld.Value;

            if (p.Status != PlayerWorldReady.StatusCode.Success)
            {
                _logger.LogWarning("World failed to prepare player {PlayerId}: {Status}", p.PlayerId, p.Status);
                SendLoginError(session, worldId, WorldLoginReturn.StatusCode.UnknownError);
                return;
            }

            var worldServer = _worldServerRegistry.Get(worldId);
            if (worldServer is null)
            {
                _logger.LogWarning("World {WorldId} went offline before player {PlayerId} could enter", worldId, p.PlayerId);
                SendLoginError(session, worldId, WorldLoginReturn.StatusCode.ServerOffline);
                return;
            }

            session.CompleteWorldTransfer();

            using var response = new PacketWriter<WorldLoginReturn>(session.Address);
            ref var rData = ref response.Data;
            rData.WorldId = worldId;
            rData.Status = WorldLoginReturn.StatusCode.Success;
            rData.WorldServerAddress = worldServer.PublicAddress;
            _packetSender.Send(response.Build());
        }

        private void SendLoginError(ClientSession session, WorldId worldId, WorldLoginReturn.StatusCode status)
        {
            if (status == WorldLoginReturn.StatusCode.Success)
            {
                throw new ArgumentException("A login error requires a failure status", nameof(status));
            }

            session.AbortWorldTransfer();

            using var response = new PacketWriter<WorldLoginReturn>(session.Address);
            ref var rData = ref response.Data;
            rData.WorldId = worldId;
            rData.Status = status;
            _packetSender.Send(response.Build());
        }
    }
}
