using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using PacketItemList = FOMServer.Shared.Core.Packets.Types.ItemList;

namespace FOMServer.World.Core.Players
{
    internal class ItemBag : ItemContainer
    {
        private readonly Dictionary<uint, Item> _items = [];
        private readonly Dictionary<ItemType, Dictionary<uint, Item>> _itemsByType = [];

        public ItemBag(Player? owner, ItemLocation location, uint locationId, IDictionary<uint, Item> items) : base(owner, location, locationId)
        {
            foreach (var (_, item) in items)
            {
                Insert(item);
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
                item.ChangeOwner(null, ItemLocation.None, 0);

                removed = item;
            }

            RaiseOnItemRemoved(removed);

            return removed;
        }

        public bool WriteTo(ref PacketItemList p)
        {
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
                if (items[i].WriteTo(ref p.Items[i]))
                {
                    i++;
                }
            }
            p.ItemCount = (uint)i;

            return true;
        }

        protected override bool Insert(Item item)
        {
            if (!item.BelongsIn(Owner, Location, LocationId))
            {
                throw new ArgumentException(
                    $"Item {item} does not match bag (owner={Owner?.Id}, location={Location}, locationId={LocationId})",
                    nameof(item));
            }

            if (!_items.TryAdd(item.Id, item))
            {
                return false;
            }

            InsertTypeIndex(item);
            return true;
        }

        protected override Item? Extract(uint id)
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
