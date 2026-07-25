using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Packets;

namespace FOMServer.World.Core.Players
{
    internal delegate void ItemDestroyedInEquipmentHandler(PlayerEquipment equipment, Item item);

    internal class PlayerEquipment : IItemLocation, IPersistableProvider
    {
        private readonly Player _player;
        private readonly Dictionary<ItemSlotType, ItemSlot> _itemSlots;

        public PlayerEquipment(Player player, IDictionary<uint, Item> items)
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
                else
                {
                    throw new ArgumentException($"Item {item} does not belong in equipment");
                }
            }

            _itemSlots = [];
            foreach (var slotType in validSlotTypes)
            {
                slotItems.TryGetValue(slotType, out var item);

                ItemSlot slot;
                if (slotType is >= ItemSlotType.WeaponStart and < ItemSlotType.WeaponEnd)
                {
                    slot = new WeaponSlot(this, slotType, item);
                }
                else if (slotType is >= ItemSlotType.EquipmentStart and < ItemSlotType.EquipmentEnd)
                {
                    slot = new EquipmentSlot(this, slotType, item);
                }
                else
                {
                    slot = new ItemSlot(this, slotType, item);
                }

                slot.ItemDestroyed += OnItemDestroyed;

                _itemSlots[slotType] = slot;
            }
        }

        public event ItemDestroyedInEquipmentHandler? ItemDestroyed;

        public uint PlayerId => _player.Id;

        public ItemLocationRef LocationRef => new(ItemLocationType.Equipment, _player.Id, _player);

        public void CollectPersistables(ICollection<IPersistable> destination)
        {
            foreach (var slot in _itemSlots.Values)
            {
                slot.CollectPersistables(destination);
            }
        }

        public IEnumerable<ItemContainer> GetItemContainers()
        {
            foreach (var slot in _itemSlots.Values)
            {
                yield return slot;
            }
        }

        public ItemContainer? GetItemContainer(ItemSlotType slotType)
        {
            var containerType = slotType.GetContainerType();
            if (containerType is ItemContainerType.Weapons or ItemContainerType.Equipment)
            {
                if (!_itemSlots.TryGetValue(slotType, out var slot))
                {
                    return null;
                }

                return slot;
            }

            return null;
        }

        public void WriteTo(
            ref RegisterClientReturnPacket.WeaponsArray weapons,
            ref RegisterClientReturnPacket.EquipmentArray equipment
        )
        {
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

        private class WeaponSlot : ItemSlot
        {
            public WeaponSlot(IItemLocation location, ItemSlotType slotType, Item? item)
                : base(location, slotType, item) { }
        }

        private class EquipmentSlot : ItemSlot
        {
            public EquipmentSlot(IItemLocation location, ItemSlotType slotType, Item? item)
                : base(location, slotType, item) { }
        }
    }
}
