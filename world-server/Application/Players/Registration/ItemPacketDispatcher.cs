using FOMServer.Shared.Core.Items;
using FOMServer.World.Core.Players.Registration;

namespace FOMServer.World.Application.Players.Registration
{
    internal class ItemPacketDispatcher : IItemPacketDispatcher
    {
        private readonly ILogger<ItemPacketDispatcher> _logger;

        public ItemPacketDispatcher(ILogger<ItemPacketDispatcher> logger)
        {
            _logger = logger;
        }

        public void Register(IItemLocation location)
        {
            foreach (var container in location.GetItemContainers())
            {
                container.ItemAdded += OnItemAdded;
                container.ItemRemoved += OnItemRemoved;
                container.ItemTransferredTo += OnItemTransferredTo;
                container.ItemTransferredFrom += OnItemTransferredFrom;
            }
        }

        public void Unregister(IItemLocation location)
        {
            foreach (var container in location.GetItemContainers())
            {
                container.ItemAdded -= OnItemAdded;
                container.ItemRemoved -= OnItemRemoved;
                container.ItemTransferredTo -= OnItemTransferredTo;
                container.ItemTransferredFrom -= OnItemTransferredFrom;
            }
        }

        private void OnItemAdded(ItemContainer container, Item item)
        {
            var loc = container.Location.Location;
            var slot = container.SlotType;

            _logger.LogInformation($"Item {item.Id} Added To {loc.Type} - {loc.Id} Slot {slot}");
        }

        private void OnItemRemoved(ItemContainer container, Item item)
        {
            var loc = container.Location.Location;
            var slot = container.SlotType;

            _logger.LogInformation($"Item {item.Id} Removed From {loc.Type} - {loc.Id} Slot {slot}");
        }

        private void OnItemTransferredTo(ItemContainer container, Item item, ItemContainer to)
        {
            var loc = container.Location.Location;
            var locTo = to.Location.Location;
            var slot = container.SlotType;
            var slotTo = to.SlotType;

            _logger.LogInformation($"Item {item.Id} Move To {locTo.Type} - {locTo.Id} Slot {slotTo} From {loc.Type} - {loc.Id} Slot {slot}");
        }

        private void OnItemTransferredFrom(ItemContainer container, Item item, ItemContainer from)
        {
            var loc = container.Location.Location;
            var locFrom = from.Location.Location;
            var slot = container.SlotType;
            var slotFrom = from.SlotType;

            _logger.LogInformation($"Item {item.Id} Move From {locFrom.Type} - {locFrom.Id} Slot {slotFrom} To {loc.Type} - {loc.Id} Slot {slot}");
        }
    }
}
