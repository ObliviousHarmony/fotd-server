using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class DisconnectionHandler : PacketHandlerBase<DisconnectionNotification>
    {
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogger<DisconnectionHandler> _logger;

        public DisconnectionHandler(
            IWorldServerRegistry worldServerRegistry,
            ILogger<DisconnectionHandler> logger)
        {
            _worldServerRegistry = worldServerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotification p)
        {
            if (TryWorldServerUnregister(sender))
                return;
        }

        private bool TryWorldServerUnregister(NetworkAddress sender)
        {
            var unregistered = _worldServerRegistry.Unregister(sender);
            if (unregistered.Length == 0)
                return false;

            foreach (var worldID in unregistered)
                _logger.LogInformation("World '{WorldID}' Disconnected", worldID);

            return true;
        }
    }
}
