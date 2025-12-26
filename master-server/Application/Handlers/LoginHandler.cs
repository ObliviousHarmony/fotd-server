using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class LoginHandler : PacketHandlerBase<Login>
    {
        private readonly ILogService _logService;
        private readonly ILoginService _loginService;
        private readonly IClientPacketSender _packetSender;

        public LoginHandler(
            ILogService logService,
            ILoginService loginService,
            IClientPacketSender packetSender)
        {
            _logService = logService;
            _loginService = loginService;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login p)
        {
            _logService.WriteMessage(LogLevel.Info, $"Player '{p.Username}' Attempting Login");

            var player = _loginService.Login(p.Username, p.PasswordHash, sender);
            if (player == null)
                return;

            _logService.WriteMessage(LogLevel.Info, $"Player '{p.Username}' ({player.ID}) Logged In");
        }
    }
}
