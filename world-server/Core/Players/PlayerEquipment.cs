using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using PacketEquipment = FOMServer.Shared.Core.Packets.RegisterClientReturn.EquipmentArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerEquipment
    {
        private readonly Lock _syncRoot = new();

        private readonly Player _player;
        private readonly Slot[] _equipment;

        public PlayerEquipment(Player player, IDictionary<uint, Item> items)
        {
            _player = player;

            _equipment = new Slot[PlayerConstants.NumEquipmentSlots];
            foreach (var (_, item) in items)
            {
                var slot = (EquipmentSlot)item.LocationId;
                if (!Enum.IsDefined(slot))
                {
                    throw new ArgumentException($"Item {item.Id} is an invalid slot ({item.LocationId}");
                }

                if (_equipment[(uint)slot] is not null)
                {
                    throw new ArgumentException($"Slot {slot} is already occupied by an item");
                }

                _equipment[(uint)slot] = new Slot(player, slot, item);
            }

            for (var i = 0; i < PlayerConstants.NumEquipmentSlots; ++i)
            {
                if (_equipment[i] is null)
                {
                    _equipment[i] = new Slot(player, (EquipmentSlot)i, null);
                }
            }
        }

        public bool WriteTo(ref PacketEquipment p)
        {
            lock (_syncRoot)
            {
                for (var i = 0; i < PlayerConstants.NumEquipmentSlots; ++i)
                {
                    _equipment[i].WriteTo(ref p[i]);
                }
            }

            return true;
        }

        private class Slot : ItemSlot
        {
            public Slot(Player owner, EquipmentSlot slot, Item? item) : base(owner, ItemLocation.Equipment, (uint)slot, item)
            {
            }
        }
    }
}
