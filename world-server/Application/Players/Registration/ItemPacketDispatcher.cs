using System.Numerics;
using System.Reflection;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Players.Registration;

namespace FOMServer.World.Application.Players.Registration
{
    internal class ItemPacketDispatcher : IItemPacketDispatcher
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<ItemPacketDispatcher> _logger;

        public ItemPacketDispatcher(IPlayerRegistry playerRegistry, IClientPacketSender clientPacketSender, ILogger<ItemPacketDispatcher> logger)
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public void Register(IItemLocation location)
        {
            foreach (var container in location.GetItemContainers())
            {
                container.ItemsAdded += OnItemsAdded;
                container.ItemsRemoved += OnItemsRemoved;
                container.ItemsTransferredOutOf += OnItemsTransferredOutOf;
                container.ItemsTransferredInto += OnItemsTransferredInto;
                container.ItemDestroyed += OnItemDestroyed;
            }
        }

        public void Unregister(IItemLocation location)
        {
            foreach (var container in location.GetItemContainers())
            {
                container.ItemsAdded -= OnItemsAdded;
                container.ItemsRemoved -= OnItemsRemoved;
                container.ItemsTransferredOutOf -= OnItemsTransferredOutOf;
                container.ItemsTransferredInto -= OnItemsTransferredInto;
                container.ItemDestroyed -= OnItemDestroyed;
            }
        }

        private void OnItemsAdded(ItemContainer container, IReadOnlyCollection<Item> items)
        {
            var locationRef = container.Location.LocationRef;
            if (!locationRef.IsPlayer())
            {
                return;
            }

            var player = _playerRegistry.Get(locationRef.Id);
            if (player is null)
            {
                return;
            }

            foreach (var item in items)
            {
                _logger.LogInformation("Player {PlayerId} ID_ITEMS_ADDED - {Type} {ItemId}: {Location} ({LocationId})", player.Id, item.Type, item.Id, locationRef.Type, locationRef.Id);
            }
        }

        private void OnItemsRemoved(ItemContainer container, IReadOnlyCollection<Item> items)
        {
            var locationRef = container.Location.LocationRef;
            if (!locationRef.IsPlayer())
            {
                return;
            }

            var player = _playerRegistry.Get(locationRef.Id);
            if (player is null)
            {
                return;
            }

            foreach (var item in items)
            {
                _logger.LogInformation("Player {PlayerId} ID_ITEMS_REMOVED - {Type} {ItemId}: {Location} ({LocationId})", player.Id, item.Type, item.Id, locationRef.Type, locationRef.Id);
            }
        }

        private void OnItemsTransferredOutOf(ItemContainer container, IReadOnlyCollection<Item> items, ItemContainer to)
        {
            var fromRef = container.Location.LocationRef;
            if (!fromRef.IsPlayer())
            {
                return;
            }

            // When an item is transferred between containers that are controlled by the
            // same player, the receiving container's event will handle the transfer.
            var toRef = container.Location.LocationRef;
            if (toRef.IsPlayer(fromRef.Id))
            {
                return;
            }

            // When an item is transferred from a container that a player controls to
            // one that they do not, the item needs to be removed from their client.
            var player = _playerRegistry.Get(fromRef.Id);
            if (player is null)
            {
                return;
            }

            foreach (var item in items)
            {
                _logger.LogInformation("Player {PlayerId} ID_ITEMS_REMOVED - {Type} {ItemId}: {Location} ({LocationId})", player.Id, item.Type, item.Id, fromRef.Type, fromRef.Id);
            }
        }

        private void OnItemsTransferredInto(ItemContainer container, IReadOnlyCollection<Item> items, ItemContainer from)
        {
            var toRef = container.Location.LocationRef;
            if (!toRef.IsPlayer())
            {
                return;
            }

            var player = _playerRegistry.Get(toRef.Id);
            if (player is null)
            {
                return;
            }

            // When an item is transferred from a container that a player does not control
            // to one that they do control, the item needs to be created on their client.
            var fromRef = container.Location.LocationRef;
            if (!fromRef.IsPlayer(toRef.Id))
            {
                foreach (var item in items)
                {
                    _logger.LogInformation("Player {PlayerId} ID_ITEMS_ADDED - {Type} {ItemId}: {Location} ({LocationId})", player.Id, item.Type, item.Id, toRef.Type, toRef.Id);
                }
                return;
            }

            // When an item is transferred between containers that are controlled by
            // the same player, the item should be moved on the player's client.
            using var response = new PacketWriter<MoveItems>(player.Address);
            ref var rData = ref response.Data;

            rData.PlayerId = player.Id;
            rData.From = PlayerInventory.GetContainerType(from.SlotType);
            rData.FromSlot = from.SlotType;
            rData.To = PlayerInventory.GetContainerType(container.SlotType);
            rData.ToSlot = container.SlotType;

            ushort i = 0;
            foreach (var item in items)
            {
                unsafe
                {
                    rData.RawItemIds[i++] = item.Id;
                }
            }
            rData.NumItemIds = i;
  
            _clientPacketSender.Send(response.Build());
        }

        private void OnItemDestroyed(ItemContainer container, Item item)
        {
            var locationRef = container.Location.LocationRef;
            if (!locationRef.IsPlayer())
            {
                return;
            }

            var player = _playerRegistry.Get(locationRef.Id);
            if (player is null)
            {
                return;
            }

            _logger.LogInformation("ID_ITEM_REMOVED - {Type} {Id}: {Location} ({LocationId})", item.Type, item.Id, locationRef.Type, locationRef.Id);

            // Use that packet for deletions because the last bit will print a destroyed system message to chat.
        }
    }
}
