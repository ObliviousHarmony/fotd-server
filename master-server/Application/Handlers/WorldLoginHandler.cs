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
        private readonly IClientPacketSender _packetSender;
        private readonly IWorldPacketSender _worldPacketSender;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRepository _playerRepository;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogger<WorldLoginHandler> _logger;

        public WorldLoginHandler(
            IClientPacketSender packetSender,
            IWorldPacketSender worldPacketSender,
            IClientRegistry clientRegistry,
            IPlayerRepository playerRepository,
            IWorldServerRegistry worldServerRegistry,
            ILogger<WorldLoginHandler> logger)
        {
            _packetSender = packetSender;
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

            var player = _playerRepository.GetByID(p.PlayerID);
            if (player is null)
            {
                SendLoginError(sender, p.WorldID, WorldLoginReturn.StatusCode.UnknownError);
                return;
            }

            var worldServer = _worldServerRegistry.Get(p.WorldID);
            if (worldServer is null)
            {
                SendLoginError(sender, p.WorldID, WorldLoginReturn.StatusCode.ServerOffline);
                return;
            }

            session.BeginWorldTransfer(p.WorldID);

            using var prepareWorldServer = new PacketWriter<PlayerMigrateWorld>(worldServer.ServerAddress);
            ref var mData = ref prepareWorldServer.Data;

            mData.PlayerID = p.PlayerID;
            mData.ClientBinaryAddress = sender.BinaryAddress;

            _worldPacketSender.Send(prepareWorldServer.Build());
        }

        private void SendLoginError(in NetworkAddress destination, WorldID worldID, WorldLoginReturn.StatusCode status)
        {
            using var response = new PacketWriter<WorldLoginReturn>(destination);
            ref var rData = ref response.Data;

            rData.WorldID = worldID;
            rData.Status = status;

            _packetSender.Send(response.Build());
        }
    }
}
