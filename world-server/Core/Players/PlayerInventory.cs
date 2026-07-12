using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using PacketEquipment = FOMServer.Shared.Core.Packets.RegisterClientReturn.EquipmentArray;
using PacketInventory = FOMServer.Shared.Core.Packets.Types.ItemList;
using PacketWeapons = FOMServer.Shared.Core.Packets.RegisterClientReturn.WeaponsArray;
using QuickSlotsArray = FOMServer.Shared.Core.Packets.RegisterClientReturn.QuickSlotsArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerInventory : IItemLocation
    {
        private readonly Player _player;
        private readonly ItemBag _backpackItems;
        private readonly Dictionary<ItemSlotType, ItemSlot> _itemSlots;
        private readonly ItemType[] _quickslots;

        public PlayerInventory(Player player, IDictionary<uint, Item> items, ReadOnlySpan<ItemType> quickslots)
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
            foreach (var slotType in validSlotTypes)
            {
                slotItems.TryGetValue(slotType, out var item);
                _itemSlots[slotType] = new ItemSlot(this, slotType, item);
            }

            if (quickslots.Length != PlayerConstants.NumQuickSlots)
            {
                throw new ArgumentException($"There must be exactly {PlayerConstants.NumQuickSlots} quickslots", nameof(quickslots));
            }

            _quickslots = [.. quickslots];
        }

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

        public bool MoveQuickslotItem(ItemSlotType fromSlot, ItemSlotType toSlot, uint? itemId)
        {
            // Unlike normal item slots, quickslots only contain the type of the item that
            // should be used when the quickslot is activated. To that end, when moving
            // items from/to them, we need to get the item so we can get the type.
            int toQuickslot = toSlot - ItemSlotType.QuickslotStart;
            if (fromSlot is >= ItemSlotType.QuickslotStart and < ItemSlotType.QuickslotEnd)
            {
                var fromQuickslot = fromSlot - ItemSlotType.QuickslotStart;
                if (toSlot is >= ItemSlotType.QuickslotStart and < ItemSlotType.QuickslotEnd)
                {
                    (_quickslots[fromQuickslot], _quickslots[toQuickslot]) = (_quickslots[toQuickslot], _quickslots[fromQuickslot]);
                    return true;
                }

                _quickslots[fromQuickslot] = ItemType.Invalid;
                return true;
            }

            if (toSlot is not (>= ItemSlotType.QuickslotStart and < ItemSlotType.QuickslotEnd))
            {
                return false;
            }

            if (itemId is null)
            {
                return false;
            }

            var fromContainer = GetItemContainer(fromSlot);
            if (fromContainer is null)
            {
                return false;
            }

            var items = fromContainer.GetAll();
            foreach (var item in items)
            {
                if (item.Id == itemId)
                {
                    _quickslots[toQuickslot] = item.Type;
                    return true;
                }
            }

            return false;
        }

        public void WriteTo(ref PacketInventory inventory, ref PacketWeapons weapons, ref PacketEquipment equipment, ref QuickSlotsArray quickslots)
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

            for (var i = 0; i < PlayerConstants.NumQuickSlots; ++i)
            {
                quickslots[i] = _quickslots[i];
            }
        }
    }
}
