using FOMServer.Master.Application.FOMPacket;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;

namespace FOMServer.Master.Application.Handlers
{
    public class CreateCharacterHandler : PacketHandler<CreateCharacter>
    {
        private readonly IClientPacketSender packetSender;
        private readonly IPlayerService playerService;
        private readonly ICharacterRepository characterRepository;
        private readonly ILoginReturnFactory loginReturnFactory;

        public CreateCharacterHandler(IClientPacketSender packetSender, IPlayerService playerService, ICharacterRepository characterRepository, ILoginReturnFactory loginReturnFactory)
        {
            this.packetSender = packetSender;
            this.playerService = playerService;
            this.characterRepository = characterRepository;
            this.loginReturnFactory = loginReturnFactory;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_CREATE_CHARACTER;

        public override void Handle(NetworkAddress sender, in CreateCharacter data)
        {
            var player = playerService.Get(sender);
            if (player == null)
                return;

            var created = characterRepository.Create(
                player.ID,
                data.Avatar.Faction,
                data.Name,
                data.Biography,
                data.Avatar.Sex,
                data.Avatar.SkinColor,
                data.Avatar.Face,
                data.Avatar.Hair
            );
            if (created == null)
                throw new InvalidOperationException("Failed to create character.");

            player.HasCharacter = true;

            var response = loginReturnFactory.Create(player);

            packetSender.Send(
                PacketIdentifier.ID_LOGIN_RETURN,
                new FOMDataUnion { loginReturn = response },
                sender,
                PacketPriority.HIGH_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
