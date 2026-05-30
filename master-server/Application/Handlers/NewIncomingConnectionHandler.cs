using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
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
            if (_clientRegistry.Get(sender) is not null)
            {
                _logger.LogWarning("Ignoring duplicate connection for '{Sender}'", sender);
                return;
            }

            _clientRegistry.Register(sender);
        }
    }
}
