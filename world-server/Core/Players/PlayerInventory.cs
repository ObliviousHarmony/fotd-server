using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using PacketInventory = FOMServer.Shared.Core.Packets.Types.ItemList;
using PacketWeapons = FOMServer.Shared.Core.Packets.RegisterClientReturn.WeaponsArray;
using PacketEquipment = FOMServer.Shared.Core.Packets.RegisterClientReturn.EquipmentArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerInventory : IItemLocation
    {
        private readonly Player _player;
        private readonly ItemBag _backpackItems;
        private readonly Dictionary<ItemSlotType, ItemSlot> _itemSlots;

        public PlayerInventory(Player player, IDictionary<uint, Item> items)
        {
            _player = player;

            HashSet<ItemSlotType> validSlots = [];
            for (var i = ItemSlotType.WeaponStart; i < ItemSlotType.WeaponEnd; ++i)
            {
                validSlots.Add(i);
            }
            for (var i = ItemSlotType.EquipmentStart; i < ItemSlotType.EquipmentEnd; ++i)
            {
                validSlots.Add(i);
            }

            var inventory = new Dictionary<uint, Item>();
            var equipment = new Dictionary<ItemSlotType, Item>();
            foreach (var (_, item) in items)
            {
                var slot = item.Slot;
                if (validSlots.Contains(slot))
                {
                    if (!equipment.TryAdd(slot, item))
                    {
                        throw new ArgumentException($"Item {item} cannot be placed in occupied slot {slot}", nameof(items));
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

            _itemSlots = [];
            foreach (var slot in validSlots)
            {
                equipment.TryGetValue(slot, out var item);
                _itemSlots[slot] = new ItemSlot(this, slot, item);
            }
        }

        public ItemLocationRef Location => new(ItemLocationType.Player, _player.Id, _player);

        public ItemContainer? GetItemContainer(ItemContainerType type, ItemSlotType slotType)
        {
            if (type == ItemContainerType.Inventory)
            {
                return _backpackItems;
            }
            else if (type is ItemContainerType.Weapons or ItemContainerType.Equipment)
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
    }
}
