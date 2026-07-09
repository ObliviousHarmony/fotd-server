using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class MoveItemsHandler : PacketHandlerBase<MoveItems>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<MoveItemsHandler> _logger;

        public MoveItemsHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<MoveItemsHandler> logger)
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in MoveItems p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected packet for player {PlayerId}", p.PlayerId);
                return;
            }

            if (player.Id != p.PlayerId)
            {
                _logger.LogWarning("Received invalid packet for player {PlayerId}", p.PlayerId);
                return;
            }

            Console.WriteLine($"Player: {player.Id} Items {p.NumIds}");
            for (var i = 0; i < p.NumIds; ++i)
            {
                unsafe
                {
                    Console.WriteLine($"ID {p.Ids[i]}");
                }
            }
            Console.WriteLine($"From {p.Source} / {p.SourceIndex}");
            Console.WriteLine($"To {p.Destination} / {p.DestinationIndex}");

            using var response = new PacketWriter<MoveItems>(sender);
            ref var rData = ref response.Data;
            rData = p;
            _clientPacketSender.Send(response.Build());
        }
    }
}
