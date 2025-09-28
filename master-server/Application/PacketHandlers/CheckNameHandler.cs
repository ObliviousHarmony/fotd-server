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

        private readonly IPlayerRepository playerRepository;
        private readonly IPacketSender packetSender;

        public CheckNameHandler(IPlayerRepository playerRepository, IPacketSender packetSender)
        {
            this.playerRepository = playerRepository;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in CheckName data)
        {
            var existingID = playerRepository.FindIDByName(data.Name);

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
