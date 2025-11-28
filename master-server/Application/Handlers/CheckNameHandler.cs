using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class CheckNameHandler : BasePacketHandler<CheckName>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IClientPacketSender _packetSender;

        public CheckNameHandler(ICharacterRepository characterRepository, IClientPacketSender packetSender)
        {
            _characterRepository = characterRepository;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in CheckName p)
        {
            var existingID = _characterRepository.Exists(p.Name);

            using var response = new PacketWriter<CheckNameReturn>();
            ref var rData = ref response.Data;

            rData.ExistingPlayerID = existingID ?? 0;

            response.AddDestination(sender);
            _packetSender.Send(response.Build());
        }
    }
}
