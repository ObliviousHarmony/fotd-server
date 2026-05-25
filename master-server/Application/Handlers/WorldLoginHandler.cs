using FOMServer.Master.Core.Networking;
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
        private readonly IPlayerRepository _playerRepository;
        private readonly IWorldServerRegistry _worldServerRegistry;

        public WorldLoginHandler(
            IClientPacketSender packetSender,
            IPlayerRepository playerRepository,
            IWorldServerRegistry worldServerRegistry)
        {
            _packetSender = packetSender;
            _playerRepository = playerRepository;
            _worldServerRegistry = worldServerRegistry;
        }

        public override void Handle(NetworkAddress sender, in WorldLogin p)
        {
            using var response = new PacketWriter<WorldLoginReturn>(sender);
            ref var rData = ref response.Data;

            rData.WorldID = p.WorldID;

            var player = _playerRepository.GetByID(p.PlayerID);
            if (player is null)
            {
                rData.Status = WorldLoginReturn.StatusCode.UnknownError;
                _packetSender.Send(response.Build());
                return;
            }

            var worldServer = _worldServerRegistry.Get(p.WorldID);
            if (worldServer is null)
            {
                rData.Status = WorldLoginReturn.StatusCode.ServerOffline;
                _packetSender.Send(response.Build());
                return;
            }

            rData.Status = WorldLoginReturn.StatusCode.Success;
            rData.WorldServerAddress = worldServer.PublicAddress;
            _packetSender.Send(response.Build());
        }
    }
}
