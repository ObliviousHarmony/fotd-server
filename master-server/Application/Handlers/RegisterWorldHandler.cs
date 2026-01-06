using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class RegisterWorldPacketHandler : PacketHandlerBase<RegisterWorld>
    {
        private readonly ILogService _logService;
        private readonly IWorldServerRegistry _worldServerRegistry;

        public RegisterWorldPacketHandler(ILogService logService, IWorldServerRegistry worldServerRegistry)
        {
            _logService = logService;
            _worldServerRegistry = worldServerRegistry;
        }

        public override void Handle(NetworkAddress sender, in RegisterWorld p)
        {
            var server = _worldServerRegistry.Register(p.WorldID, sender, p.ClientAddress);
            if (server == null)
                throw new InvalidOperationException($"World '{p.WorldID}' already registered");

            _logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Connected: {server.ClientAddress}");
        }
    }
}
