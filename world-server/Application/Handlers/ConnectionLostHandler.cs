using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data.RakNetPackets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class ConnectionLostHandler : BasePacketHandler<ConnectionLost>
    {
        private readonly IPlayerRegistry _playerRegistry;

        public ConnectionLostHandler(IPlayerRegistry playerRegistry)
        {
            _playerRegistry = playerRegistry;
        }

        public override void Handle(NetworkAddress sender, in ConnectionLost p)
        {
            var player = _playerRegistry.Get(sender);
            if (player == null)
                return;

            _playerRegistry.Unregister(player.ID);
        }
    }
}
