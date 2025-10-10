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
    public class RegisterWorldPacketHandler : BasePacketHandler<RegisterWorld>
    {
        public PacketIdentifier PacketID => PacketIdentifier.ID_REGISTER_WORLD;

        private readonly ILogService _logService;
        private readonly IWorldServerService _worldServerService;

        public RegisterWorldPacketHandler(ILogService logService, IWorldServerService worldServerService)
        {
            _logService = logService;
            _worldServerService = worldServerService;
        }

        public override void Handle(NetworkAddress sender, in RegisterWorld p)
        {
            var server = _worldServerService.Register(p.WorldID, sender, p.ClientAddress);
            if (server == null)
                throw new InvalidOperationException($"World '{p.WorldID}' already registered");

            _logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Connected: {server.ClientAddress}");
        }
    }
}
