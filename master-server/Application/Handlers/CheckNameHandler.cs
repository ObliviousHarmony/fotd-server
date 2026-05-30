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
    internal class CheckNameHandler : PacketHandlerBase<CheckName>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IPlayerRepository _playerRepository;

        public CheckNameHandler(
            IClientPacketSender clientPacketSender,
            IPlayerRepository playerRepository)
        {
            _clientPacketSender = clientPacketSender;
            _playerRepository = playerRepository;
        }

        public override void Handle(NetworkAddress sender, in CheckName p)
        {
            using var response = new PacketWriter<CheckNameReturn>(sender);
            ref var rData = ref response.Data;

            var player = _playerRepository.GetByName(p.Name);
            if (player is not null)
            {
                rData.OwnerPlayerId = player.id;
            }

            _clientPacketSender.Send(response.Build());
        }
    }
}
