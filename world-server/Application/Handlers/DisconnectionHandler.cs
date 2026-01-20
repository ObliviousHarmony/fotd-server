using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class DisconnectionHandler : PacketHandlerBase<DisconnectionNotification>
    {
        public DisconnectionHandler()
        {
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotification p)
        {
        }
    }
}
