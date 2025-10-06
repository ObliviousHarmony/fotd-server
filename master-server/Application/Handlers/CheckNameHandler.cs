using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
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

        public override void Handle(NetworkAddress sender, in CheckName data)
        {
            var existingID = _characterRepository.Exists(data.Name);

            var response = new CheckNameReturn
            {
                ExistingPlayerID = existingID ?? 0
            };
            var responsePacket = new QueuePacket.PacketData<CheckNameReturn>(response);
            _packetSender.Send(responsePacket, sender, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
