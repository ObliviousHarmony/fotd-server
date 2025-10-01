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
            var worldServer = worldServerService.Register(data.WorldID, data.Address, data.Port);
            if (worldServer == null)
                throw new InvalidOperationException($"World server with ID {data.WorldID} is already registered.");

            logService.WriteMessage(LogLevel.Info, $"Registered {data.WorldID}: {data.Address}:{data.Port}");
        }
    }
}
