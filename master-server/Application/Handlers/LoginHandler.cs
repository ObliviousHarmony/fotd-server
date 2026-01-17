using System.Numerics;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Player;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class LoginHandler : PacketHandlerBase<Login>
    {
        private readonly ILoginService _loginService;
        private readonly IClientPacketSender _packetSender;

        public LoginHandler(
            ILoginService loginService,
            IClientPacketSender packetSender)
        {
            _loginService = loginService;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login p)
        {
        }
    }
}
