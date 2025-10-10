using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data.RakNetPackets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class ConnectionLostHandler : BasePacketHandler<ConnectionLost>
    {
        private readonly IPlayerService _playerService;

        public ConnectionLostHandler(IPlayerService playerService, ILogService logService)
        {
            _playerService = playerService;
        }

        public override void Handle(NetworkAddress sender, in ConnectionLost p)
        {
            Player? player = _playerService.Get(sender);
            if (player == null)
                return;

            _playerService.OnPlayerLeftWorld(player.ID);
        }
    }
}
