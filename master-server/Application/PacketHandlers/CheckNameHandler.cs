using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class CheckNameHandler : PacketHandler<CheckName>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_CHECK_NAME;

        private readonly ICharacterRepository characterRepository;
        private readonly IPacketSender packetSender;

        public CheckNameHandler(ICharacterRepository characterRepository, IPacketSender packetSender)
        {
            this.characterRepository = characterRepository;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in CheckName data)
        {
            var existingID = characterRepository.IsNameTaken(data.Name);

            var response = new CheckNameReturn
            {
                ExistingAccountID = existingID ?? 0
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
