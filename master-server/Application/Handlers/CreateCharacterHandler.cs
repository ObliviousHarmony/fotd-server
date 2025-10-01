using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class CreateCharacterHandler : PacketHandler<CreateCharacter>
    {
        private readonly IPlayerService playerService;
        private readonly ICharacterRepository characterRepository;

        public CreateCharacterHandler(IPlayerService playerService, ICharacterRepository characterRepository)
        {
            this.playerService = playerService;
            this.characterRepository = characterRepository;
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
        }
    }
}
