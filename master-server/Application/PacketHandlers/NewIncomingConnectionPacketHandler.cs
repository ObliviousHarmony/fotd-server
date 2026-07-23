using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets.RakNet;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class NewIncomingConnectionPacketHandler : PacketHandlerBase<NewIncomingConnectionPacket>
    {
        private readonly IClientRegistry _clientRegistry;
        private readonly ILogger<NewIncomingConnectionPacketHandler> _logger;

        public NewIncomingConnectionPacketHandler(
            IClientRegistry clientRegistry,
            ILogger<NewIncomingConnectionPacketHandler> logger
        )
        {
            _clientRegistry = clientRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in NewIncomingConnectionPacket p)
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
