using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets.RakNet;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.PacketHandlers
{
    [PacketHandler]
    internal class DisconnectionPacketHandler : PacketHandlerBase<DisconnectionNotificationPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger<DisconnectionPacketHandler> _logger;

        public DisconnectionPacketHandler(IPlayerRegistry playerRegistry, ILogger<DisconnectionPacketHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotificationPacket p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is not null)
            {
                _playerRegistry.Logout(player);
            }
        }
    }
}
