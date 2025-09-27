using FOMServer.Master.Application.Services;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;
using FOMServer.Shared.Core.Models.FOMData;
using FOMServer.Shared.Infrastructure.Services;

namespace FOMServer.Master.Application.PacketHandlers
{
    public class CheckNameHandler : PacketHandler<CheckName>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_CHECK_NAME;

        private readonly ILogService logService;
        private readonly IPacketSender packetSender;

        public CheckNameHandler(ILogService logService, IPacketSender packetSender)
        {
            this.logService = logService;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in CheckName data)
        {
            string name;
            unsafe
            {
                fixed (byte* ptr = data.Name)
                    name = CStringParser.ToString(ptr, 20);
            }

            logService.WriteMessage(LogLevel.Info, $"Received name check for '{name}' from {sender}");
        }
    }
}
