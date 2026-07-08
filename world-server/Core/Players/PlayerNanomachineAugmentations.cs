using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using PacketNanomachineAugmentations = FOMServer.Shared.Core.Packets.RegisterClientReturn.NanomachineAugmentationsArray;

namespace FOMServer.World.Core.Players
{
    internal class PlayerNanomachineAugmentations
    {
        private readonly Lock _syncRoot = new();

        private readonly Player _player;
        private readonly Slot[] _augmentations;

        public PlayerNanomachineAugmentations(Player player, IDictionary<uint, Item> items)
        {
            _player = player;

            _augmentations = new Slot[PlayerConstants.NumNanomachineAugmentationSlots];
            foreach (var (_, item) in items)
            {
                var slot = item.LocationId;
                if (slot >= PlayerConstants.NumNanomachineAugmentationSlots)
                {
                    throw new ArgumentException($"Item {item.Id} is an invalid slot ({item.LocationId}");
                }

                if (_augmentations[slot] is not null)
                {
                    throw new ArgumentException($"Slot {slot} is already occupied by an item");
                }

                _augmentations[slot] = new Slot(player, slot, item);
            }

            for (uint i = 0; i < PlayerConstants.NumNanomachineAugmentationSlots; ++i)
            {
                if (_augmentations[i] is null)
                {
                    _augmentations[i] = new Slot(player, i, null);
                }
            }
        }

        public bool WriteTo(ref PacketNanomachineAugmentations p)
        {
            lock (_syncRoot)
            {
                for (var i = 0; i < PlayerConstants.NumNanomachineAugmentationSlots; ++i)
                {
                    _augmentations[i].WriteTo(ref p[i]);
                }
            }

            return true;
        }

        private class Slot : ItemSlot
        {
            public Slot(Player owner, uint slot, Item? item) : base(owner, ItemLocation.NanomachineAugmentation, slot, item)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)slot, PlayerConstants.NumNanomachineAugmentationSlots);
            }
        }
    }
}
