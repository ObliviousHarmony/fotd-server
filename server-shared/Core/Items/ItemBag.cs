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

        public ItemBag(IItemLocation location, IDictionary<uint, Item> items) : base(location, ItemSlotType.None)
        {
            _maxSpace = 100;
            _reservedSpace = 0;

            foreach (var (_, item) in items)
            {
                item.BindLocation(Location, SlotType);
                InsertCore(item);
            }
        }

        public Item? RemoveOfType(ItemType type)
        {
            Item removed;
            lock (_syncRoot)
            {
                if (!_itemsByType.TryGetValue(type, out var byType) || byType.Count == 0)
                {
                    return null;
                }

                using var e = byType.GetEnumerator();
                if (!e.MoveNext())
                {
                    return null;
                }

                var (id, item) = e.Current;

                byType.Remove(id);
                _items.Remove(id);

                item.OnDestroyed -= OnItemDestroyed;
                item.Move(null, ItemSlotType.None);

                removed = item;
            }

            RaiseOnItemRemoved(removed);

            return removed;
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
                throw new InvalidOperationException($"Bag contains too many items, has {items.Length}");
            }

            var i = 0;
            foreach (var item in items)
            {
                item.WriteTo(ref p.Items[i++]);
            }
            p.ItemCount = (uint)i;
        }

        protected override Item? GetCore(uint id)
        {
            if (!_items.TryGetValue(id, out var item))
            {
                return null;
            }

            return item;
        }

        protected override bool InsertCore(Item item)
        {
            if (!_items.TryAdd(item.Id, item))
            {
                return false;
            }

            InsertTypeIndex(item);

            return true;
        }

        protected override Item? ExtractCore(uint id)
        {
            if (!_items.Remove(id, out var item))
            {
                return null;
            }

            ExtractTypeIndex(item);

            return item;
        }

        protected override void OnItemDestroyed(Item item)
        {
            lock (_syncRoot)
            {
                item.OnDestroyed -= OnItemDestroyed;
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
