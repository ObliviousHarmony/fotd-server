using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using PacketActiveConsumables = FOMServer.Shared.Core.Packets.RegisterClientReturn.ActiveConsumablesArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerActiveConsumables
    {
        private readonly Lock _syncRoot = new();

        private readonly Player _player;
        private readonly Slot[] _consumables;

        public PlayerActiveConsumables(Player player, IDictionary<uint, Item> items)
        {
            _player = player;

            _consumables = new Slot[PlayerConstants.NumActiveConsumableSlots];
            foreach (var (_, item) in items)
            {
                var slot = item.LocationId;
                if (slot >= PlayerConstants.NumActiveConsumableSlots)
                {
                    throw new ArgumentException($"Item {item.Id} is an invalid slot ({item.LocationId}");
                }

                if (_consumables[slot] is not null)
                {
                    throw new ArgumentException($"Slot {slot} is already occupied by an item");
                }

                _consumables[slot] = new Slot(player, slot, item);
            }

            for (uint i = 0; i < PlayerConstants.NumActiveConsumableSlots; ++i)
            {
                if (_consumables[i] is null)
                {
                    _consumables[i] = new Slot(player, i, null);
                }
            }
        }

        public bool WriteTo(ref PacketActiveConsumables p)
        {
            lock (_syncRoot)
            {
                for (var i = 0; i < PlayerConstants.NumActiveConsumableSlots; ++i)
                {
                    _consumables[i].WriteTo(ref p[i]);
                }
            }

            return true;
        }

        private class Slot : ItemSlot
        {
            public Slot(Player owner, uint slot, Item? item) : base(owner, ItemLocation.ActiveConsumable, slot, item)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)slot, PlayerConstants.NumActiveConsumableSlots);
            }
        }
    }
}
