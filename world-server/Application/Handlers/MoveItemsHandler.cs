using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Enums;
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

            if (p.From is ItemContainerType.Inventory or ItemContainerType.Weapons or ItemContainerType.Equipment)
            {
                if (!player.Inventory.MoveItems(p.Ids, p.From, p.FromSlot, p.To, p.ToSlot))
                {
                    _logger.LogWarning(
                        "Failed to move items {Ids} from {From} / {FromSlot} to {To} / {ToSlot}",
                        p.Ids.ToString(),
                        p.From,
                        p.FromSlot,
                        p.To,
                        p.ToSlot
                    );
                    return;
                }
            }
            else
            {
                _logger.LogError("Items cannot be moved to {To} / {ToSlot}", p.To, p.ToSlot);
                return;
            }

            using var response = new PacketWriter<MoveItems>(sender);
            ref var rData = ref response.Data;
            rData = p;
            _clientPacketSender.Send(response.Build());
        }
    }
}
