using FOMServer.World.Core.Players;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data.RakNetPackets;
using FOMServer.Shared.Metadata;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    public class DisconnectionHandler : BasePacketHandler<DisconnectionNotification>
    {
        private readonly IPlayerService _playerService;

        public DisconnectionHandler(IPlayerService playerService, ILogService logService)
        {
            _playerService = playerService;
        }

        public override void Handle(NetworkAddress sender, in DisconnectionNotification p)
        {
            Console.WriteLine($"Disconnection Lost from {sender}");
            Player? player = _playerService.Get(sender);
            if (player == null)
                return;

            Console.WriteLine($"Player {player.ID}");

            _playerService.OnPlayerLeftWorld(player.ID);
        }
    }
}
