using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;

namespace FOMServer.Master.Application.Handlers
{
    public class DisconnectionHandler : PacketHandler<RakNetPacket>
    {
        private readonly IPlayerService playerService;
        private readonly IWorldServerService worldServerService;
        private readonly ILogService logService;

        public DisconnectionHandler(IPlayerService playerService, IWorldServerService worldServerService, ILogService logService)
        {
            this.playerService = playerService;
            this.worldServerService = worldServerService;
            this.logService = logService;
        }

        public override PacketIdentifier PacketID => PacketIdentifier.ID_DISCONNECTION_NOTIFICATION;

        public override void Handle(NetworkAddress sender, in RakNetPacket data)
        {
            if (TryWorldServerUnregister(sender))
                return;

            if (TryPlayerLogout(sender))
                return;
        }

        private bool TryWorldServerUnregister(NetworkAddress sender)
        {
            var worldServers = worldServerService.GetAll();
            foreach (var server in worldServers)
            {
                if (!server.ServerAddress.Equals(sender))
                    continue;

                logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Disconnected");
                worldServerService.Unregister(server.ID);
                return true;
            }

            return false;
        }

        private bool TryPlayerLogout(NetworkAddress sender)
        {
            Player? player = playerService.Get(sender);
            if (player == null)
                return false;

            playerService.Logout(player);
            return true;
        }
    }
}
