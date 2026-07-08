using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using PacketItem = FOMServer.Shared.Core.Packets.Types.Item;

namespace FOMServer.World.Core.Players
{
    internal class ItemSlot : ItemContainer
    {
        protected Item? _item;

        public ItemSlot(Player owner, ItemLocation location, uint slot, Item? item) : base(owner, location, slot)
        {
            if (item is not null)
            {
                Insert(item);
            }
        }

        public bool WriteTo(ref PacketItem p)
        {
            lock (_syncRoot)
            {
                if (_item is null)
                {
                    return false;
                }

                _item.WriteTo(ref p);
            }

            return true;
        }

        protected override Item? Extract(uint id)
        {
            if (_item?.Id != id)
            {
                return null;
            }

            var item = _item;
            _item = null;
            return item;
        }

        protected override bool Insert(Item item)
        {
            if (_item is not null)
            {
                return false;
            }

            if (item is not null && !item.BelongsIn(Location, LocationId))
            {
                throw new ArgumentException(
                        $"Item {item} does not match slot (location={Location}, locationId={LocationId})",
                        nameof(item));
            }

            _item = item;
            return true;
        }

        protected override void OnItemDestroyed(Item item)
        {
            if (ReferenceEquals(item, _item))
            {
                _item = null;
            }
        }
    }
}
