using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class CheckNameHandler : PacketHandlerBase<CheckName>
    {
        private readonly IClientPacketSender _packetSender;
        private readonly IPlayerRepository _playerRepository;

        public CheckNameHandler(
            IClientPacketSender packetSender,
            IPlayerRepository playerRepository)
        {
            _packetSender = packetSender;
            _playerRepository = playerRepository;
        }

        public override void Handle(NetworkAddress sender, in CheckName p)
        {
            using var response = new PacketWriter<CheckNameReturn>(sender);
            ref var rData = ref response.Data;

            var player = _playerRepository.GetByName(p.Name);
            if (player != null)
                rData.OwnerPlayerID = player.id;

            _packetSender.Send(response.Build());
        }
    }
}
