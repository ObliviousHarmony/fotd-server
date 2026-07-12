using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class WorldLoginHandler : PacketHandlerBase<WorldLogin>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IWorldPacketSender _worldPacketSender;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRepository _playerRepository;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogger<WorldLoginHandler> _logger;

        public WorldLoginHandler(
            IClientPacketSender clientPacketSender,
            IWorldPacketSender worldPacketSender,
            IClientRegistry clientRegistry,
            IPlayerRepository playerRepository,
            IWorldServerRegistry worldServerRegistry,
            ILogger<WorldLoginHandler> logger)
        {
            _clientPacketSender = clientPacketSender;
            _worldPacketSender = worldPacketSender;
            _clientRegistry = clientRegistry;
            _playerRepository = playerRepository;
            _worldServerRegistry = worldServerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in WorldLogin p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                _logger.LogWarning("Dropping unexpected world login from '{Sender}'", sender);
                return;
            }

            if (session.PlayerId != p.PlayerId)
            {
                _logger.LogWarning("Inventory {PlayerId} attempted world login on session belonging to {SessionPlayerId}", p.PlayerId, session.PlayerId);
                SendLoginError(sender, p.WorldId, WorldLoginReturn.StatusCode.UnknownError);
                return;
            }

            if (_playerRepository.GetById(p.PlayerId) is null)
            {
                SendLoginError(sender, p.WorldId, WorldLoginReturn.StatusCode.UnknownError);
                return;
            }

            var destinationServer = _worldServerRegistry.Get(p.WorldId);
            if (destinationServer is null)
            {
                SendLoginError(sender, p.WorldId, WorldLoginReturn.StatusCode.ServerOffline);
                return;
            }

            session.BeginWorldTransfer(p.WorldId);

            WorldServer? currentServer = null;
            if (session.CurrentWorld.HasValue)
            {
                currentServer = _worldServerRegistry.Get(session.CurrentWorld.Value);
            }

            if (currentServer is not null)
            {
                using var leaving = new PacketWriter<PlayerLeavingWorld>(currentServer.ServerAddress);
                leaving.Data.PlayerId = p.PlayerId;
                _worldPacketSender.Send(leaving.Build());
                return;
            }

            using var migrate = new PacketWriter<PlayerMigrateWorld>(destinationServer.ServerAddress);
            ref var mData = ref migrate.Data;
            mData.PlayerId = p.PlayerId;
            mData.ClientBinaryAddress = sender.BinaryAddress;
            _worldPacketSender.Send(migrate.Build());
        }

        private void SendLoginError(in NetworkAddress destination, WorldId worldId, WorldLoginReturn.StatusCode status)
        {
            if (status == WorldLoginReturn.StatusCode.Success)
            {
                throw new ArgumentException("A login error requires a failure status", nameof(status));
            }

            using var response = new PacketWriter<WorldLoginReturn>(destination);
            ref var rData = ref response.Data;

            rData.WorldId = worldId;
            rData.Status = status;

            _clientPacketSender.Send(response.Build());
        }
    }
}
