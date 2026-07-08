using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using PacketEquipment = FOMServer.Shared.Core.Packets.RegisterClientReturn.EquipmentArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerEquipment
    {
        private readonly Lock _syncRoot = new();

        private readonly Player _player;
        private readonly Slot[] _equipment;

        public PlayerEquipment(Player player, Dictionary<EquipmentSlot, Item> items)
        {
            _player = player;

            _equipment = new Slot[(uint)EquipmentSlot.NUM_EQUIPMENT_SLOTS];
            for (EquipmentSlot i = 0; i < EquipmentSlot.NUM_EQUIPMENT_SLOTS; ++i)
            {
                items.TryGetValue(i, out var item);
                _equipment[(uint)i] = new Slot(player, i, item);
            }
        }

        public bool WriteTo(ref PacketEquipment p)
        {
            lock (_syncRoot)
            {
                for (var i = 0; i < (uint)EquipmentSlot.NUM_EQUIPMENT_SLOTS; ++i)
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
