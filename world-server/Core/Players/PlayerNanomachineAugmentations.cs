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
            for (uint i = 0; i < PlayerConstants.NumNanomachineAugmentationSlots; ++i)
            {
                items.TryGetValue(i, out var item);
                _augmentations[i] = new Slot(player, i, item);
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
