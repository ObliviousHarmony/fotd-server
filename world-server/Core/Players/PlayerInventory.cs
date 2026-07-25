using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Structs.Item;

namespace FOMServer.World.Core.Players
{
    internal class PlayerInventory : IItemLocation, IPersistableProvider
    {
        private readonly Player _player;
        private readonly ItemBag _backpackItems;

        public PlayerInventory(Player player, IDictionary<uint, Item> items)
        {
            _player = player;

            foreach (var (_, item) in items)
            {
                var slot = item.Slot;
                if (slot != ItemSlotType.None)
                {
                    throw new ArgumentException($"Item {item} does not belong in the inventory");
                }
            }

            _backpackItems = new ItemBag(this, items);
        }

        public uint PlayerId => _player.Id;

        public ItemLocationRef LocationRef => new(ItemLocationType.Inventory, _player.Id, _player);

        public void CollectPersistables(ICollection<IPersistable> destination)
        {
            _backpackItems.CollectPersistables(destination);
        }

        public IEnumerable<ItemContainer> GetItemContainers()
        {
            yield return _backpackItems;
        }

        public ItemContainer? GetItemContainer(ItemSlotType slotType)
        {
            if (slotType.GetContainerType() == ItemContainerType.Inventory)
            {
                return _backpackItems;
            }

            return null;
        }

        public void WriteTo(ref ItemListInterop inventory)
        {
            _backpackItems.WriteTo(ref inventory);
        }
    }
}
