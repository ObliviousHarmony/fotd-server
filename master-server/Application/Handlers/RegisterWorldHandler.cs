using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;

namespace FOMServer.Master.Application.Handlers
{
    public class RegisterWorldPacketHandler : PacketHandler<RegisterWorld>
    {
        private readonly ILogService logService;
        private readonly IWorldServerService worldServerService;

        public override PacketIdentifier PacketID => PacketIdentifier.ID_REGISTER_WORLD;

        public RegisterWorldPacketHandler(ILogService logService, IWorldServerService worldServerService)
        {
            this.logService = logService;
            this.worldServerService = worldServerService;
        }

        public override void Handle(NetworkAddress sender, in RegisterWorld data)
        {
            var server = worldServerService.Register(data.WorldID, sender, data.Address, data.Port);
            if (server == null)
                throw new InvalidOperationException($"World '{data.WorldID}' Already Registered");

            logService.WriteMessage(LogLevel.Info, $"World '{server.ID}' Connected: {server.ClientAddress}");
        }
    }
}
