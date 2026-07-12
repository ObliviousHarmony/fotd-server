using System;
using System.Collections.Generic;
using System.Text;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using PacketQuickslots = FOMServer.Shared.Core.Packets.RegisterClientReturn.QuickSlotsArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerQuickslots : IPersistable
    {
        private readonly Player _player;
        private readonly ItemType[] _quickslots;

        public PlayerQuickslots(Player player, ReadOnlySpan<ItemType> quickslots)
        {
            _player = player;

            if (quickslots.Length != PlayerConstants.NumQuickslots)
            {
                throw new ArgumentException(
                    $"There must be exactly {PlayerConstants.NumQuickslots} quickslots",
                    nameof(quickslots)
                );
            }

            _quickslots = [.. quickslots];
        }

        public event PersistableChangeCallback? PersistableChange;

        public uint PlayerId => _player.Id;

        public void WriteTo(ref PacketQuickslots quickslots)
        {
            for (var i = 0; i < PlayerConstants.NumQuickslots; ++i)
            {
                quickslots[i] = _quickslots[i];
            }
        }

        public bool PutItemInSlot(ItemSlotType fromSlot, ItemSlotType toSlot, uint? itemId)
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
                    (_quickslots[fromQuickslot], _quickslots[toQuickslot]) = (
                        _quickslots[toQuickslot],
                        _quickslots[fromQuickslot]
                    );
                    PersistableChange?.Invoke(this, _player);
                    return true;
                }

                _quickslots[fromQuickslot] = ItemType.Invalid;
                PersistableChange?.Invoke(this, _player);
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

            var fromContainer = _player.Inventory.GetItemContainer(fromSlot);
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
                    PersistableChange?.Invoke(this, _player);
                    return true;
                }
            }

            return false;
        }
    }
}
