using FOMServer.Master.Core.Accounts;
using FOMServer.Master.Core.Repositories;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
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
