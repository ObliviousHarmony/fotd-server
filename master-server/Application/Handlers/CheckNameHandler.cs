using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class CheckNameHandler : PacketHandler<CheckName>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_CHECK_NAME;

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
            _packetSender.Send(response, sender, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
