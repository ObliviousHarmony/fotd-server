using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Tick;
using WorldUpdateData = FOMServer.Shared.Core.Packets.Types.WorldUpdate;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class UpdateHandler : PacketHandlerBase<Update>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPlayerUpdateTick _playerUpdateService;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            IPlayerRegistry playerRegistry,
            IPlayerUpdateTick playerUpdateService,
            ILogger<UpdateHandler> logger
        )
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

            if (p.WorldUpdate.Kind != WorldUpdateData.Type.Player)
            {
                _logger.LogWarning(
                    "Unexpected update of type {updateKind} from player {PlayerId}",
                    p.WorldUpdate.Kind,
                    player.Id
                );
                return;
            }

            player.ApplyUpdate(p.WorldUpdate.Player);
            _playerUpdateService.QueueUpdate(player);
        }
    }
}
