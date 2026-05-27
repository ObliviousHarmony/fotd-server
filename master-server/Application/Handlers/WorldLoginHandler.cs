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
                _logger.LogWarning("Dropping world login from '{Address}' with no registered session", sender);
                return;
            }

            var player = _playerRepository.GetById(p.PlayerId);
            if (player is null)
            {
                SendLoginError(sender, p.WorldId, WorldLoginReturn.StatusCode.UnknownError);
                return;
            }

            var worldServer = _worldServerRegistry.Get(p.WorldId);
            if (worldServer is null)
            {
                SendLoginError(sender, p.WorldId, WorldLoginReturn.StatusCode.ServerOffline);
                return;
            }

            session.BeginWorldTransfer(p.WorldId);

            using var prepareWorldServer = new PacketWriter<PlayerMigrateWorld>(worldServer.ServerAddress);
            ref var mData = ref prepareWorldServer.Data;

            mData.PlayerId = p.PlayerId;
            mData.ClientBinaryAddress = sender.BinaryAddress;

            _worldPacketSender.Send(prepareWorldServer.Build());
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
