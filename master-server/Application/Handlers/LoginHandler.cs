using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class LoginHandler : PacketHandlerBase<Login>
    {
        private readonly IClientPacketSender _packetSender;

        public LoginHandler(
            IClientPacketSender packetSender)
        {
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login p)
        {
        }
    }
}
