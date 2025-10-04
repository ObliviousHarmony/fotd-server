using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data.RakNetPackets;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler(PacketIdentifier.ID_DISCONNECTION_NOTIFICATION)]
    public class DisconnectionHandler : BasePacketHandler<DisconnectionNotification>
    {
        private readonly IPlayerService _playerService;
        private readonly IWorldServerService _worldServerService;
        private readonly ILogService _logService;

        public DisconnectionHandler(IPlayerService playerService, IWorldServerService worldServerService, ILogService logService)
        {
            _playerService = playerService;
            _worldServerService = worldServerService;
            _logService = logService;
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotification data)
        {
            if (TryWorldServerUnregister(sender))
                return;

            if (TryPlayerLogout(sender))
                return;
        }

        private bool TryWorldServerUnregister(NetworkAddress sender)
        {
            var worldServers = _worldServerService.GetAll();
            foreach (var server in worldServers)
            {
                if (!server.ServerAddress.Equals(sender))
                    continue;

                _logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Disconnected");
                _worldServerService.Unregister(server.ID);
                return true;
            }

            return false;
        }

        private bool TryPlayerLogout(NetworkAddress sender)
        {
            Player? player = _playerService.Get(sender);
            if (player == null)
                return false;

            _playerService.Logout(player);
            return true;
        }
    }
}
