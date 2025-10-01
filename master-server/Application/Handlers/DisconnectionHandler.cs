using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;

namespace FOMServer.Master.Application.Handlers
{
    public class DisconnectionHandler : PacketHandler<RakNetPacket>
    {
        private readonly IPlayerService playerService;
        private readonly ILogService logService;

        public DisconnectionHandler(IPlayerService playerService, ILogService logService)
        {
            this.playerService = playerService;
            this.logService = logService;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_DISCONNECTION_NOTIFICATION;

        public override void Handle(NetworkAddress sender, in RakNetPacket data)
        {
            Player? player = playerService.Get(sender);
            if (player == null)
                return;

            if (!playerService.Logout(player))
                logService.WriteMessage(LogLevel.Critical, $"Player '{player.Username}' could not be logged out on disconnection from {sender}");
        }
    }
}
