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

        private readonly ICharacterRepository characterRepository;
        private readonly IClientPacketSender packetSender;

        public CheckNameHandler(ICharacterRepository characterRepository, IClientPacketSender packetSender)
        {
            this.characterRepository = characterRepository;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in CheckName data)
        {
            var existingID = characterRepository.Exists(data.Name);

            var response = new CheckNameReturn
            {
                ExistingPlayerID = existingID ?? 0
            };
            packetSender.Send(
                PacketIdentifier.ID_CHECK_NAME_RETURN,
                new FOMDataUnion { checkNameReturn = response },
                sender,
                PacketPriority.MEDIUM_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
