using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;
using WorldUpdate = FOMServer.Shared.Core.Packets.WorldUpdate;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class UpdateHandler : PacketHandlerBase<Update>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<UpdateHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
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

            using var response = new PacketWriter<WorldUpdate>(sender);
            ref var data = ref response.Data;

            data.PlayerId = player.Id;
            data.UpdateCount = 1;
            data.Updates[0] = p.WorldUpdate;
            data.Updates[0].Type = WorldUpdateType.Character;

            _clientPacketSender.Send(response.Build());
        }
    }
}
