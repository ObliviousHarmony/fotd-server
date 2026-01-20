using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class ConnectionLostHandler : PacketHandlerBase<ConnectionLost>
    {
        private readonly IWorldServerRegistry _worldServerRegistry;
        private readonly ILogService _logService;

        public ConnectionLostHandler(
            IWorldServerRegistry worldServerRegistry,
            ILogService logService)
        {
            _worldServerRegistry = worldServerRegistry;
            _logService = logService;
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
                _logService.WriteMessage(LogLevel.Info, $"World '{worldID}' Lost Connection");

            return true;
        }
    }
}
