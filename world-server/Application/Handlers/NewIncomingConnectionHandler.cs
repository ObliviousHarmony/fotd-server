using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class NewIncomingConnectionHandler : PacketHandlerBase<NewIncomingConnection>
    {
        private readonly IClientRegistry _clientRegistry;
        private readonly ILogger<NewIncomingConnectionHandler> _logger;

        public NewIncomingConnectionHandler(
            IClientRegistry clientRegistry,
            ILogger<NewIncomingConnectionHandler> logger)
        {
            _clientRegistry = clientRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in NewIncomingConnection p)
        {
            // RakNet can re-deliver NewIncomingConnection for an already-connected peer
            // (e.g. loopback), so guard against a duplicate registration.
            if (_clientRegistry.Get(sender) is not null)
            {
                _logger.LogWarning("Ignoring duplicate connection for '{Address}'", sender);
                return;
            }

            _clientRegistry.Register(sender);
            _logger.LogInformation("Client '{Address}' connected", sender);
        }
    }
}
