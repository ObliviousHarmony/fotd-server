using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums.Item;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using PacketEquipment = FOMServer.Shared.Core.Packets.RegisterClientReturn.EquipmentArray;
using PacketInventory = FOMServer.Shared.Core.Packets.Types.Item.ItemList;
using PacketWeapons = FOMServer.Shared.Core.Packets.RegisterClientReturn.WeaponsArray;

namespace FOMServer.World.Core.Players
{
    internal delegate void ItemDestroyedInInventory(PlayerInventory inventory, Item item);

    internal class PlayerInventory : IItemLocation
    {
        private readonly Player _player;
        private readonly ItemBag _backpackItems;
        private readonly Dictionary<ItemSlotType, ItemSlot> _itemSlots;

        public PlayerInventory(Player player, IDictionary<uint, Item> items)
        {
            _player = player;

            HashSet<ItemSlotType> validSlotTypes = [];
            for (var i = ItemSlotType.WeaponStart; i < ItemSlotType.WeaponEnd; ++i)
            {
                validSlotTypes.Add(i);
            }
            for (var i = ItemSlotType.EquipmentStart; i < ItemSlotType.EquipmentEnd; ++i)
            {
                validSlotTypes.Add(i);
            }

            var inventory = new Dictionary<uint, Item>();
            var slotItems = new Dictionary<ItemSlotType, Item>();
            foreach (var (_, item) in items)
            {
                var slot = item.Slot;
                if (validSlotTypes.Contains(slot))
                {
                    if (!slotItems.TryAdd(slot, item))
                    {
                        throw new ArgumentException(
                            $"Item {item} cannot be placed in occupied slot {slot}",
                            nameof(items)
                        );
                    }
                }
                else if (slot == ItemSlotType.None)
                {
                    inventory[item.Id] = item;
                }
                else
                {
                    throw new ArgumentException($"Item {item} does not belong in the inventory");
                }
            }

            _backpackItems = new ItemBag(this, inventory);
            _backpackItems.ItemDestroyed += OnItemDestroyed;

            _itemSlots = [];
            foreach (var slotType in validSlotTypes)
            {
                slotItems.TryGetValue(slotType, out var item);
                _itemSlots[slotType] = new ItemSlot(this, slotType, item);
                _itemSlots[slotType].ItemDestroyed += OnItemDestroyed;
            }
        }

        public event ItemDestroyedInInventory? ItemDestroyed;

        public uint PlayerId => _player.Id;

        public ItemLocationRef LocationRef => new(ItemLocationType.Inventory, _player.Id, _player);

        public static ItemContainerType GetContainerType(ItemSlotType slotType)
        {
            if (slotType == ItemSlotType.None)
            {
                return ItemContainerType.Inventory;
            }

            if (slotType is >= ItemSlotType.WeaponStart and < ItemSlotType.WeaponEnd)
            {
                return ItemContainerType.Weapons;
            }

            if (slotType is >= ItemSlotType.EquipmentStart and < ItemSlotType.EquipmentEnd)
            {
                return ItemContainerType.Equipment;
            }

            return ItemContainerType.None;
        }

        public IEnumerable<ItemContainer> GetItemContainers()
        {
            yield return _backpackItems;
            foreach (var slot in _itemSlots.Values)
            {
                yield return slot;
            }
        }

        public ItemContainer? GetItemContainer(ItemSlotType slotType)
        {
            var containerType = GetContainerType(slotType);
            if (containerType == ItemContainerType.Inventory)
            {
                return _backpackItems;
            }
            else if (containerType is ItemContainerType.Weapons or ItemContainerType.Equipment)
            {
                if (!_itemSlots.TryGetValue(slotType, out var slot))
                {
                    return null;
                }

                return slot;
            }

            return null;
        }

        public void WriteTo(ref PacketInventory inventory, ref PacketWeapons weapons, ref PacketEquipment equipment)
        {
            _backpackItems.WriteTo(ref inventory);

            for (var slot = ItemSlotType.WeaponStart; slot < ItemSlotType.WeaponEnd; ++slot)
            {
                _itemSlots[slot].WriteTo(ref weapons[slot - ItemSlotType.WeaponStart]);
            }

            for (var slot = ItemSlotType.EquipmentStart; slot < ItemSlotType.EquipmentEnd; ++slot)
            {
                _itemSlots[slot].WriteTo(ref equipment[slot - ItemSlotType.EquipmentStart]);
            }
        }

        private void OnItemDestroyed(ItemContainer itemContainer, Item item)
        {
            ItemDestroyed?.Invoke(this, item);
        }
    }
}
