using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class ConnectionLostHandler : PacketHandlerBase<ConnectionLost>
    {
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger<ConnectionLostHandler> _logger;

        public ConnectionLostHandler(
            IWorldServerRegistry worldServerRegistry,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            ILogger<ConnectionLostHandler> logger)
        {
            _worldServerRegistry = worldServerRegistry;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in ConnectionLost p)
        {
            if (TryWorldServerUnregister(sender))
            {
                return;
            }

            TryClientUnregister(sender);
        }

        private bool TryWorldServerUnregister(NetworkAddress sender)
        {
            var unregistered = _worldServerRegistry.Unregister(sender);
            if (unregistered.Length == 0)
            {
                return false;
            }

            foreach (var worldID in unregistered)
            {
                _logger.LogInformation("World '{WorldID}' lost connection", worldID);
            }

            return true;
        }

        private void TryClientUnregister(NetworkAddress sender)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                return;
            }

            if (session.Player is not null)
            {
                _playerRegistry.Logout(session.Player);
            }

            _ = _clientRegistry.Unregister(session);
            _logger.LogInformation("Client '{Address}' lost connection", sender);
        }
    }
}
