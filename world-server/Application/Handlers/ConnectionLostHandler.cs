using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class ConnectionLostHandler : PacketHandlerBase<ConnectionLost>
    {
        public ConnectionLostHandler()
        {
        }

        public override void Handle(NetworkAddress sender, in ConnectionLost p)
        {
        }
    }
}
