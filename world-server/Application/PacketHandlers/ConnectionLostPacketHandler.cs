using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets.RakNet;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.PacketHandlers
{
    [PacketHandler]
    internal class ConnectionLostPacketHandler : PacketHandlerBase<ConnectionLostPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger<ConnectionLostPacketHandler> _logger;

        public ConnectionLostPacketHandler(IPlayerRegistry playerRegistry, ILogger<ConnectionLostPacketHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in ConnectionLostPacket p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is not null)
            {
                _playerRegistry.Logout(player);
            }
        }
    }
}
