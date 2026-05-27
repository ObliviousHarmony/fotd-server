using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
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
            var session = _clientRegistry.Get(p.PlayerID);
            if (session is null)
            {
                _logger.LogWarning("Received world-ready for unknown player {PlayerID}", p.PlayerID);
                return;
            }

            if (!session.PendingWorld.HasValue)
            {
                _logger.LogWarning("Received world-ready for player {PlayerID} with no transfer in progress", p.PlayerID);
                return;
            }

            var worldID = session.PendingWorld.Value;

            using var response = new PacketWriter<WorldLoginReturn>(session.Address);
            ref var rData = ref response.Data;
            rData.WorldID = worldID;

            if (p.Status != PlayerWorldReady.StatusCode.Success)
            {
                _logger.LogWarning("World failed to prepare player {PlayerID}: {Status}", p.PlayerID, p.Status);
                session.CompleteWorldTransfer(false);
                rData.Status = WorldLoginReturn.StatusCode.UnknownError;
                _packetSender.Send(response.Build());
                return;
            }

            var worldServer = _worldServerRegistry.Get(worldID);
            if (worldServer is null)
            {
                _logger.LogWarning("World {WorldID} went offline before player {PlayerID} could enter", worldID, p.PlayerID);
                session.CompleteWorldTransfer(false);
                rData.Status = WorldLoginReturn.StatusCode.ServerOffline;
                _packetSender.Send(response.Build());
                return;
            }

            session.CompleteWorldTransfer(success: true);
            rData.Status = WorldLoginReturn.StatusCode.Success;
            rData.WorldServerAddress = worldServer.PublicAddress;
            _packetSender.Send(response.Build());
        }
    }
}
