using System.Diagnostics;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using PacketItem = FOMServer.Shared.Core.Packets.Types.Item;

namespace FOMServer.Shared.Core.Items
{
    public class ItemSlot : ItemContainer
    {
        protected Item? _item;

        public ItemSlot(IItemLocation location, ItemSlotType slotType, Item? item)
            : base(location, slotType)
        {
            if (item is not null)
            {
                InsertCore(item);
                item.BindLocation(Location, SlotType);
                item.ItemDestroyed += OnItemDestroyed;
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

        protected override IReadOnlyCollection<Item> GetAllCore()
        {
            if (_item is null)
            {
                return [];
            }

            return [_item];
        }

        protected override IReadOnlyCollection<uint> GetDisplacedIdsFor(params IReadOnlyCollection<uint> idsToInsert)
        {
            if (idsToInsert.Count > 1)
            {
                return [];
            }

            // When there's an item, in order to insert it into this container, it
            // needs to displace the one that is already inside of the container.
            if (_item is null)
            {
                return [];
            }

            return [_item.Id];
        }

        protected override bool CanInsertCore(params IReadOnlyCollection<uint> idsToInsert)
        {
            return _item is null;
        }

        protected override bool CanInsertCore(
            IReadOnlyCollection<uint> idsToInsert,
            IReadOnlyCollection<uint> idsToExtract
        )
        {
            if (idsToInsert.Count == 0)
            {
                return true;
            }

            if (_item is null)
            {
                return true;
            }

            // When an item already occupies the slot, the only way to insert a new
            // item is if we're going to be displacing the item already there.
            if (idsToExtract.Count != 1)
            {
                return false;
            }

            return _item.Id == idsToExtract.First();
        }

        protected override bool InsertCore(params IReadOnlyCollection<Item> itemsToInsert)
        {
            if (itemsToInsert.Count == 0)
            {
                return true;
            }

            if (_item is not null)
            {
                return false;
            }

            if (itemsToInsert.Count != 1)
            {
                return false;
            }

            _item = itemsToInsert.First();
            return true;
        }

        protected override bool CanExtractCore(params IReadOnlyCollection<uint> idsToExtract)
        {
            if (idsToExtract.Count == 0)
            {
                return true;
            }

            if (_item is null)
            {
                return false;
            }

            if (idsToExtract.Count != 1)
            {
                return false;
            }

            return _item.Id == idsToExtract.First();
        }

        protected override IReadOnlyCollection<Item> ExtractCore(params IReadOnlyCollection<uint> idsToExtract)
        {
            if (idsToExtract.Count != 1)
            {
                return [];
            }

            if (_item is null)
            {
                return [];
            }

            var item = _item;
            _item = null;
            return [item];
        }

        protected override void OnItemDestroyedCore(Item item)
        {
            lock (_syncRoot)
            {
                if (ReferenceEquals(item, _item))
                {
                    _item = null;
                }
            }
        }
    }
}
