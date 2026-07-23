using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class CheckNamePacketHandler : PacketHandlerBase<CheckNamePacket>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IPlayerRepository _playerRepository;

        public CheckNamePacketHandler(IClientPacketSender clientPacketSender, IPlayerRepository playerRepository)
        {
            _clientPacketSender = clientPacketSender;
            _playerRepository = playerRepository;
        }

        public override void Handle(NetworkAddress sender, in CheckNamePacket p)
        {
            using var response = new PacketWriter<CheckNameReturnPacket>(sender);
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
