using FOMServer.Master.Application.Services;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class CreateCharacterHandler : PacketHandler<CreateCharacter>
    {
        private readonly IAccountService accountService;
        private readonly ICharacterRepository characterRepository;

        public CreateCharacterHandler(IAccountService accountService, ICharacterRepository characterRepository)
        {
            this.accountService = accountService;
            this.characterRepository = characterRepository;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_CREATE_CHARACTER;

        public override void Handle(NetworkAddress sender, in CreateCharacter data)
        {
            var account = accountService.Get(sender);
            if (account == null)
                return;

            var created = characterRepository.Create(
                account.ID,
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
