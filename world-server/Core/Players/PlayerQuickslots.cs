using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Packets;

namespace FOMServer.World.Core.Players
{
    internal class PlayerQuickslots : IPersistable
    {
        private readonly Lock _syncRoot = new();

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

        public event PersistableChangeHandler? PersistableChange;

        public uint PlayerId => _player.Id;

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
                    lock (_syncRoot)
                    {
                        (_quickslots[fromQuickslot], _quickslots[toQuickslot]) = (
                            _quickslots[toQuickslot],
                            _quickslots[fromQuickslot]
                        );
                    }

                    PersistableChange?.Invoke(this, _player);
                    return true;
                }

                lock (_syncRoot)
                {
                    _quickslots[fromQuickslot] = ItemType.Invalid;
                }

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

            if (!fromContainer.TryGetItemSnapshot(itemId.Value, out var snapshot))
            {
                return false;
            }

            lock (_syncRoot)
            {
                _quickslots[toQuickslot] = snapshot.Type;
            }

            PersistableChange?.Invoke(this, _player);
            return true;
        }

        public void CopyTo(Span<ItemType> destination)
        {
            lock (_syncRoot)
            {
                _quickslots.CopyTo(destination);
            }
        }

        public void WriteTo(ref RegisterClientReturnPacket.QuickSlotsArray quickslots)
        {
            lock (_syncRoot)
            {
                for (var i = 0; i < PlayerConstants.NumQuickslots; ++i)
                {
                    quickslots[i] = _quickslots[i];
                }
            }
        }
    }
}
