using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using PacketWeapons = FOMServer.Shared.Core.Packets.RegisterClientReturn.WeaponsArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerWeapons
    {
        private readonly Lock _syncRoot = new();

        private readonly Player _player;
        private readonly Slot[] _weapons;

        public PlayerWeapons(Player player, IDictionary<uint, Item> items)
        {
            _player = player;

            _weapons = new Slot[PlayerConstants.NumWeaponSlots];
            foreach (var (_, item) in items)
            {
                var slot = item.LocationId;
                if (slot >= PlayerConstants.NumWeaponSlots)
                {
                    throw new ArgumentException($"Item {item.Id} is an invalid slot ({item.LocationId}");
                }

                if (_weapons[slot] is not null)
                {
                    throw new ArgumentException($"Slot {slot} is already occupied by an item");
                }

                _weapons[slot] = new Slot(player, slot, item);
            }

            for (uint i = 0; i < PlayerConstants.NumWeaponSlots; ++i)
            {
                if (_weapons[i] is null)
                {
                    _weapons[i] = new Slot(player, i, null);
                }
            }
        }

        public bool WriteTo(ref PacketWeapons p)
        {
            lock (_syncRoot)
            {
                for (var i = 0; i < PlayerConstants.NumWeaponSlots; ++i)
                {
                    _weapons[i].WriteTo(ref p[i]);
                }
            }

            return true;
        }

        private class Slot : ItemSlot
        {
            public Slot(Player owner, uint slot, Item? item) : base(owner, ItemLocation.Weapons, slot, item)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)slot, PlayerConstants.NumWeaponSlots);
            }
        }
    }
}
