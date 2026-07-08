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

        public PlayerActiveConsumables(Player player, Dictionary<uint, Item> items)
        {
            _player = player;

            _consumables = new Slot[PlayerConstants.NumActiveConsumableSlots];
            for (uint i = 0; i < PlayerConstants.NumActiveConsumableSlots; ++i)
            {
                items.TryGetValue(i, out var item);
                _consumables[i] = new Slot(player, i, item);
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
