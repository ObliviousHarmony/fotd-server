using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets.RakNet;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Player;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class ConnectionLostHandler : PacketHandlerBase<ConnectionLost>
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
