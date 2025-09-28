using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class CreateCharacterHandler : PacketHandler<CreateCharacter>
    {
        private readonly ILogService logService;

        public CreateCharacterHandler(ILogService logService)
        {
            this.logService = logService;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_CREATE_CHARACTER;

        public override void Handle(NetworkAddress sender, in CreateCharacter data)
        {
            logService.WriteMessage(LogLevel.Info, $"Received character creation request from {sender} for character name '{data.Name}'");
            logService.WriteMessage(LogLevel.Info, $"Biography:{data.Biography}");

            logService.WriteMessage(LogLevel.Info, $"Sex:{data.Avatar.Sex}");
            logService.WriteMessage(LogLevel.Info, $"SkinColor:{data.Avatar.SkinColor}");
        }
    }
}
