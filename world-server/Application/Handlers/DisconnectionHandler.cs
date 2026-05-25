using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class DisconnectionHandler : PacketHandlerBase<DisconnectionNotification>
    {
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger<DisconnectionHandler> _logger;

        public DisconnectionHandler(
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            ILogger<DisconnectionHandler> logger)
        {
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotification p)
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
            _logger.LogInformation("Client '{Address}' disconnected", sender);
        }
    }
}
