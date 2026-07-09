using FOMServer.Shared.Core.Enums;
using PacketItem = FOMServer.Shared.Core.Packets.Types.Item;

namespace FOMServer.Shared.Core.Items
{
    public class ItemSlot : ItemContainer
    {
        protected Item? _item;

        public ItemSlot(IItemLocation location, ItemSlotType slotType, Item? item) : base(location, slotType)
        {
            if (item is not null)
            {
                item.BindLocation(Location, SlotType);
                Insert(item);
            }
        }

        public void WriteTo(ref PacketItem p)
        {
            lock (_syncRoot)
            {
                if (_item is null)
                {
                    return;
                }

                _item.WriteTo(ref p);
            }
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
