using System.Numerics;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

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

            _logger.LogError("Attempting to move {ItemIds} from {From} / {FromSlot} to {To} / {ToSlot}", string.Join(", ", p.ItemIds.ToArray()), p.From, p.FromSlot, p.To, p.ToSlot);

            using var response = new PacketWriter<MoveItems>(sender);
            response.Data = p;
            _clientPacketSender.Send(response.Build());

            bool success;
            if (p.From == p.To)
            {
                success = MoveBetweenSlots(player, p.From, p.FromSlot, p.ToSlot);
            }
            else if (p.To == ItemContainerType.Destroy)
            {
                success = DestroyItem(player, p.From, p.FromSlot, p.ItemIds);
            }
            else
            {
                success = MoveBetweenContainers(player, p.From, p.FromSlot, p.To, p.ToSlot, p.ItemIds);
            }

            if (!success)
            {
                _logger.LogError("Failed to move items {ItemIds} from {From} / {FromSlot} to {To} / {ToSlot}", string.Join(", ", p.ItemIds.ToArray()), p.From, p.FromSlot, p.To, p.ToSlot);
                return;
            }
        }

        private bool MoveBetweenSlots(Player player, ItemContainerType containerType, ItemSlotType fromSlot, ItemSlotType toSlot)
        {
            // Since quickslots don't actually hold any items, we need to handle them differently than we would normal items.
            if (containerType == ItemContainerType.Quickslots)
            {
                // todo: Have the player.Inventory object perform some special thing for these kinds and then send a reply packet.
                return true;
            }

            // todo: When an item is moved between slots on the same container, the client expects the server to
            // infer the item's ID from the slot in question.
            return true;
        }

        private bool DestroyItem(Player player, ItemContainerType fromType, ItemSlotType fromSlot, ReadOnlySpan<uint> itemIds)
        {
            // Since quickslots don't actually hold any items, we need to handle them differently than we would normal items.
            if (fromType == ItemContainerType.Quickslots)
            {
                // todo: Have the player.Inventory object perform some special thing for these kinds and then send a reply packet.
                return true;
            }

            // todo: When an item is destroyed it needs to be removed from the container.
            // However, unlike a normal removal, instead of sending ID_ITEMS_REMOVED,
            // we want to send an item moved packet.
            return true;
        }

        private bool MoveBetweenContainers(Player player, ItemContainerType fromType, ItemSlotType fromSlot, ItemContainerType toType, ItemSlotType toSlot, ReadOnlySpan<uint> itemIds)
        {
            var fromLocation = GetLocation(fromType, player);
            if (fromLocation is null)
            {
                _logger.LogError("Items cannot be moved from {From} / {FromSlot}", fromType, fromSlot);
                return false;
            }

            var fromContainer = fromLocation.GetItemContainer(fromSlot);
            if (fromContainer is null)
            {
                _logger.LogError($"Location {fromType} does not have container {fromSlot} to move from");
                return false;
            }

            var toLocation = GetLocation(toType, player);
            if (toLocation is null)
            {
                _logger.LogError("Items cannot be moved to {To} / {ToSlot}", toType, toSlot);
                return false;
            }

            var toContainer = toLocation.GetItemContainer(toSlot);
            if (toContainer is null)
            {
                _logger.LogError($"Location {toType} does not have container {toSlot} to move into");
                return false;
            }

            return fromContainer.TryTransfer(toContainer, out _, itemIds);
        }

        private IItemLocation? GetLocation(ItemContainerType containerType, Player player)
        {
            if (containerType is ItemContainerType.Inventory or ItemContainerType.Weapons or ItemContainerType.Equipment)
            {
                return player.Inventory;
            }

            return null;
        }
    }
}
