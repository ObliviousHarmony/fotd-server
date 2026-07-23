using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Interop.FOMNetwork.Structs;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Tick;

namespace FOMServer.World.Application.PacketHandlers
{
    [PacketHandler]
    internal class UpdatePacketHandler : PacketHandlerBase<UpdatePacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPlayerUpdateTick _playerUpdateService;
        private readonly ILogger<UpdatePacketHandler> _logger;

        public UpdatePacketHandler(
            IPlayerRegistry playerRegistry,
            IPlayerUpdateTick playerUpdateService,
            ILogger<UpdatePacketHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _playerUpdateService = playerUpdateService;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in UpdatePacket p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received update from unregistered client '{Sender}'", sender);
                return;
            }

            if (p.WorldUpdate.Kind != WorldUpdateInterop.Type.Player)
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
