using FOMServer.Master.Core.Networking;
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
        private readonly ILogger<ConnectionLostHandler> _logger;

        public ConnectionLostHandler(
            IWorldServerRegistry worldServerRegistry,
            ILogger<ConnectionLostHandler> logger)
        {
            _worldServerRegistry = worldServerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in ConnectionLost p)
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
                _logger.LogInformation("World '{WorldID}' Lost Connection", worldID);

            return true;
        }
    }
}
