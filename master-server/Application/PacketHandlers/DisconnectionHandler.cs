using FOMServer.Master.Application.Services;
using FOMServer.Master.Core.Models;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class DisconnectionHandler : PacketHandler<RakNetPacket>
    {
        private readonly IAccountService accountService;
        private readonly ILogService logService;

        public DisconnectionHandler(IAccountService accountService, ILogService logService)
        {
            this.accountService = accountService;
            this.logService = logService;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_DISCONNECTION_NOTIFICATION;

        public override void Handle(NetworkAddress sender, in RakNetPacket data)
        {
            Account? account = accountService.Get(sender);
            if (account == null)
                return;

            if (!accountService.Logout(account))
                logService.WriteMessage(LogLevel.Critical, $"Account '{account.Username}' could not be logged out on disconnection from {sender}");
        }
    }
}
