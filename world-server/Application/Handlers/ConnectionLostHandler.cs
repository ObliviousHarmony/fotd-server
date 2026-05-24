using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class ConnectionLostHandler : PacketHandlerBase<ConnectionLost>
    {
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger<ConnectionLostHandler> _logger;

        public ConnectionLostHandler(
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            ILogger<ConnectionLostHandler> logger)
        {
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in ConnectionLost p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
                return;

            if (session.Player is not null)
                _playerRegistry.Logout(session.Player);

            _clientRegistry.Unregister(session);
            _logger.LogInformation("Client '{Address}' lost connection", sender);
        }
    }
}
