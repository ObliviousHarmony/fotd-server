using FOMServer.Master.Core.Accounts;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;

namespace FOMServer.Master.Application.Handlers
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
