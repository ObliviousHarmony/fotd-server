using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Application.World;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class WorldServiceHandler : PacketHandlerBase<WorldService>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<WorldServiceHandler> _logger;

        public WorldServiceHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<WorldServiceHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in WorldService p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected packet for player {PlayerId}", p.PlayerId);
                return;
            }

            if (p.Action != WorldService.ActionType.Open)
            {
                return;
            }

            var serviceType = DummyWorldServices.GetFromId(p.Id);

            using var response = new PacketWriter<WorldService>(sender);
            ref var rData = ref response.Data;
            rData.PlayerId = p.PlayerId;
            rData.Action = WorldService.ActionType.Opened;
            rData.ServiceId = p.Id;
            rData.ServiceType = serviceType;
            _clientPacketSender.Send(response.Build());
        }
    }
}
