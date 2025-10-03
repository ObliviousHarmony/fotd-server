using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;

namespace FOMServer.Master.Application.Handlers
{
    public class RegisterWorldPacketHandler : PacketHandler<RegisterWorld>
    {
        private readonly ILogService _logService;
        private readonly IWorldServerService _worldServerService;

        public override PacketIdentifier PacketID => PacketIdentifier.ID_REGISTER_WORLD;

        public RegisterWorldPacketHandler(ILogService logService, IWorldServerService worldServerService)
        {
            this._logService = logService;
            this._worldServerService = worldServerService;
        }

        public override void Handle(NetworkAddress sender, in RegisterWorld data)
        {
            var server = _worldServerService.Register(data.WorldID, sender, data.Address, data.Port);
            if (server == null)
                throw new InvalidOperationException($"World '{data.WorldID}' Already Registered");

            _logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Connected: {server.ClientAddress}");
        }
    }
}
