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
            for (uint i = 0; i < PlayerConstants.NumWeaponSlots; ++i)
            {
                items.TryGetValue(i, out var item);
                _weapons[i] = new Slot(player, i, item);
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
