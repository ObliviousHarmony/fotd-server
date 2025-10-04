using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler(PacketIdentifier.ID_REGISTER_WORLD)]
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

        public override void Handle(NetworkAddress sender, in RegisterWorld data)
        {
            var server = _worldServerService.Register(data.WorldID, sender, data.Address, data.Port);
            if (server == null)
                throw new InvalidOperationException($"World '{data.WorldID}' already registered");

            _logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Connected: {server.ClientAddress}");
        }
    }
}
