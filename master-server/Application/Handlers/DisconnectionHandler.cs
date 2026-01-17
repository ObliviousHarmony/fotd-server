using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class DisconnectionHandler : PacketHandlerBase<DisconnectionNotification>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILoginService _loginService;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogService _logService;

        public DisconnectionHandler(
            IPlayerRegistry playerRegistry,
            ILoginService loginService,
            IWorldServerRegistry worldServerRegistry,
            ILogService logService)
        {
            _playerRegistry = playerRegistry;
            _loginService = loginService;
            _worldServerRegistry = worldServerRegistry;
            _logService = logService;
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotification p)
        {
            if (TryWorldServerUnregister(sender))
                return;

            if (TryPlayerLogout(sender))
                return;
        }

        private bool TryWorldServerUnregister(NetworkAddress sender)
        {
            var unregistered = _worldServerRegistry.Unregister(sender);
            if (unregistered.Length == 0)
                return false;

            foreach (var worldID in unregistered)
                _logService.WriteMessage(LogLevel.Info, $"World '{worldID}' Disconnected");

            return true;
        }

        private bool TryPlayerLogout(NetworkAddress sender)
        {
            Player? player = _playerRegistry.Get(sender);
            if (player == null)
                return false;

            _loginService.Logout(player);
            return true;
        }
    }
}
