using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Application.World;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.PacketHandlers
{
    [PacketHandler]
    internal class WorldServicePacketHandler : PacketHandlerBase<WorldServicePacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<WorldServicePacketHandler> _logger;

        public WorldServicePacketHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<WorldServicePacketHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in WorldServicePacket p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected packet for player {PlayerId}", p.PlayerId);
                return;
            }

            if (p.Action != WorldServicePacket.ActionType.Open)
            {
                return;
            }

            var serviceType = DummyWorldServices.GetFromId(p.Id);

            using var response = new PacketWriter<WorldServicePacket>(sender);
            ref var rData = ref response.Data;
            rData.PlayerId = p.PlayerId;
            rData.Action = WorldServicePacket.ActionType.Opened;
            rData.ServiceId = p.Id;
            rData.ServiceType = serviceType;
            _clientPacketSender.Send(response.Build());
        }
    }
}
