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
        private readonly ICharacterRepository characterRepository;

        public CreateCharacterHandler(ICharacterRepository characterRepository, ILogService logService) : base(logService)
        {
            this.characterRepository = characterRepository;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_CREATE_CHARACTER;

        public override void Handle(NetworkAddress sender, in CreateCharacter data)
        {
            characterRepository.Create(
                data.AccountID,
                data.Avatar.Faction,
                data.Name,
                data.Avatar.Sex,
                data.Avatar.SkinColor,
                data.Avatar.Face,
                data.Avatar.Hair
            );
        }
    }
}
