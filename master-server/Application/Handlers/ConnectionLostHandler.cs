using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data.RakNetPackets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class ConnectionLostHandler : PacketHandlerBase<ConnectionLost>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILoginService _loginService;
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogService _logService;

        public ConnectionLostHandler(
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

        public override void Handle(NetworkAddress sender, in ConnectionLost p)
        {
            if (TryWorldServerUnregister(sender))
                return;

            if (TryPlayerLogout(sender))
                return;
        }

        private bool TryWorldServerUnregister(NetworkAddress sender)
        {
            var worldServers = _worldServerRegistry.GetAll();
            foreach (var server in worldServers)
            {
                if (!server.ServerAddress.Equals(sender))
                    continue;

                _logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Lost Connection");
                _worldServerRegistry.Unregister(server.ID);
                return true;
            }

            return false;
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
