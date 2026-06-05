using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class UpdateHandler : PacketHandlerBase<Update>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPlayerUpdateService _playerUpdateService;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            IPlayerRegistry playerRegistry,
            IPlayerUpdateService playerUpdateService,
            ILogger<UpdateHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _playerUpdateService = playerUpdateService;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in Update p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received update from unregistered client '{Sender}'", sender);
                return;
            }

            player.ApplyUpdate(p.WorldUpdate);
            _playerUpdateService.QueueUpdate(player);
        }
    }
}
