using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class PlayerWorldReadyPacketHandler : PacketHandlerBase<PlayerWorldReadyPacket>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IClientRegistry _clientRegistry;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogger<PlayerWorldReadyPacketHandler> _logger;

        public PlayerWorldReadyPacketHandler(
            IClientPacketSender clientPacketSender,
            IClientRegistry clientRegistry,
            IWorldServerRegistry worldServerRegistry,
            ILogger<PlayerWorldReadyPacketHandler> logger
        )
        {
            _clientPacketSender = clientPacketSender;
            _clientRegistry = clientRegistry;
            _worldServerRegistry = worldServerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in PlayerWorldReadyPacket p)
        {
            var session = _clientRegistry.Get(p.PlayerId);
            if (session is null)
            {
                _logger.LogWarning("Received world-ready for unknown player {PlayerId}", p.PlayerId);
                return;
            }

            if (!session.PendingWorld.HasValue)
            {
                _logger.LogWarning("Received unexpected world-ready for player {PlayerId}", p.PlayerId);
                return;
            }

            var worldId = session.PendingWorld.Value;

            if (p.Status != PlayerWorldReadyPacket.StatusCode.Success)
            {
                _logger.LogWarning("World failed to prepare player {PlayerId}: {Status}", p.PlayerId, p.Status);
                SendLoginError(session, worldId, WorldLoginReturnPacket.StatusCode.UnknownError);
                return;
            }

            var worldServer = _worldServerRegistry.Get(worldId);
            if (worldServer is null)
            {
                _logger.LogWarning(
                    "World {WorldId} went offline before player {PlayerId} could enter",
                    worldId,
                    p.PlayerId
                );
                SendLoginError(session, worldId, WorldLoginReturnPacket.StatusCode.ServerOffline);
                return;
            }

            session.CompleteWorldTransfer();

            using var response = new PacketWriter<WorldLoginReturnPacket>(session.Address);
            ref var rData = ref response.Data;
            rData.WorldId = worldId;
            rData.Status = WorldLoginReturnPacket.StatusCode.Success;
            rData.WorldServerAddress = worldServer.PublicAddress;
            _clientPacketSender.Send(response.Build());
        }

        private void SendLoginError(ClientSession session, WorldId worldId, WorldLoginReturnPacket.StatusCode status)
        {
            if (status == WorldLoginReturnPacket.StatusCode.Success)
            {
                throw new ArgumentException("A login error requires a failure status", nameof(status));
            }

            session.AbortWorldTransfer();

            using var response = new PacketWriter<WorldLoginReturnPacket>(session.Address);
            ref var rData = ref response.Data;
            rData.WorldId = worldId;
            rData.Status = status;
            _clientPacketSender.Send(response.Build());
        }
    }
}
