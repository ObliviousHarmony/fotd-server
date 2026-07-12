using System.Diagnostics;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using PacketItemList = FOMServer.Shared.Core.Packets.Types.ItemList;

namespace FOMServer.Shared.Core.Items
{
    public class ItemBag : ItemContainer
    {
        private readonly uint _maxSpace;
        private readonly ushort _reservedSpace;
        private readonly Dictionary<uint, Item> _items = [];
        private readonly Dictionary<ItemType, Dictionary<uint, Item>> _itemsByType = [];

        public ItemBag(IItemLocation location, IDictionary<uint, Item> items)
            : base(location, ItemSlotType.None)
        {
            _maxSpace = 100;
            _reservedSpace = 0;

            foreach (var (_, item) in items)
            {
                InsertCore(item);
                item.BindLocation(Location, SlotType);
                item.ItemDestroyed += OnItemDestroyed;
            }
        }

        public void WriteTo(ref PacketItemList p)
        {
            p.MaxSpace = _maxSpace;
            p.ReservedSpace = _reservedSpace;

            Item[] items;
            lock (_syncRoot)
            {
                items = [.. _items.Values];
            }

            if (items.Length > BufferSizes.MaxItemListSize)
            {
                throw new InvalidOperationException($"Bag contains too many itemsToInsert, has {items.Length}");
            }

            var i = 0;
            foreach (var item in items)
            {
                item.WriteTo(ref p.Items[i++]);
            }
            p.ItemCount = (uint)i;
        }

        protected override IReadOnlyCollection<Item> GetAllCore()
        {
            return _items.Values;
        }

        protected override IReadOnlyCollection<uint> GetDisplacedIdsFor(params IReadOnlyCollection<uint> idsToInsert)
        {
            // Bags don't support item displacement during transfer.
            return [];
        }

        protected override bool CanInsertCore(params IReadOnlyCollection<uint> idsToInsert)
        {
            foreach (var id in idsToInsert)
            {
                if (_items.ContainsKey(id))
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool CanInsertCore(
            IReadOnlyCollection<uint> idsToInsert,
            IReadOnlyCollection<uint> idsToExtract
        )
        {
            // Bags don't support item displacement during transfer.
            if (idsToExtract.Count != 0)
            {
                return false;
            }

            if (_items.Count + idsToInsert.Count > _maxSpace)
            {
                return false;
            }

            return CanInsertCore(idsToInsert);
        }

        protected override bool InsertCore(params IReadOnlyCollection<Item> itemsToInsert)
        {
            if (itemsToInsert.Count == 0)
            {
                return true;
            }

            if (_items.Count + itemsToInsert.Count > _maxSpace)
            {
                return false;
            }

            var failure = false;
            List<Item> added = new(itemsToInsert.Count);
            foreach (var item in itemsToInsert)
            {
                if (!_items.TryAdd(item.Id, item))
                {
                    failure = true;
                    break;
                }

                InsertTypeIndex(item);

                added.Add(item);
            }

            // Make sure to remove any items we added if the insertion failed.
            if (failure)
            {
                foreach (var item in added)
                {
                    if (_items.Remove(item.Id))
                    {
                        ExtractTypeIndex(item);
                    }
                }

                return false;
            }

            return true;
        }

        protected override bool CanExtractCore(params IReadOnlyCollection<uint> idsToExtract)
        {
            foreach (var id in idsToExtract)
            {
                if (!_items.ContainsKey(id))
                {
                    return false;
                }
            }

            return true;
        }

        protected override IReadOnlyCollection<Item> ExtractCore(params IReadOnlyCollection<uint> idsToExtract)
        {
            List<Item> extracted = [];

            var failure = false;
            foreach (var id in idsToExtract)
            {
                if (!_items.Remove(id, out var item))
                {
                    failure = true;
                    break;
                }

                ExtractTypeIndex(item);

                extracted.Add(item);
            }

            // Make sure to add any items we removed back if the extraction fails.
            if (failure)
            {
                foreach (var item in extracted)
                {
                    if (_items.TryAdd(item.Id, item))
                    {
                        InsertTypeIndex(item);
                    }
                }

                return [];
            }

            return extracted;
        }

        protected override void OnItemDestroyed(Item item)
        {
            lock (_syncRoot)
            {
                item.ItemDestroyed -= OnItemDestroyed;
                _items.Remove(item.Id);
                ExtractTypeIndex(item);
            }
        }

        private void InsertTypeIndex(Item item)
        {
            if (!_itemsByType.TryGetValue(item.Type, out var byType))
            {
                byType = [];
                _itemsByType[item.Type] = byType;
            }

            byType[item.Id] = item;
        }

        private void ExtractTypeIndex(Item item)
        {
            if (_itemsByType.TryGetValue(item.Type, out var byType))
            {
                byType.Remove(item.Id);
            }
        }
    }
}
