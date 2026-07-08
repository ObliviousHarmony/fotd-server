using System.Threading;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using PacketItemList = FOMServer.Shared.Core.Packets.Types.ItemList;

namespace FOMServer.World.Core.Players
{
    internal class ItemBag
    {
        private static long s_nextLockId;
        private readonly long _lockId = Interlocked.Increment(ref s_nextLockId);

        private readonly Lock _syncRoot = new();
        private readonly Dictionary<uint, Item> _items = [];
        private readonly Dictionary<ItemType, Dictionary<uint, Item>> _itemsByType = [];

        public ItemBag(Player? owner, ItemLocation location, uint locationId, ReadOnlySpan<Item> items)
        {
            Owner = owner;
            Location = location;
            LocationId = locationId;

            foreach (var item in items)
            {
                if (!item.BelongsIn(owner, Location, LocationId))
                {
                    throw new ArgumentException(
                        $"Item {item} does not match bag (owner={owner?.Id}, location={Location}, locationId={LocationId})",
                        nameof(items));
                }

                if (!_items.TryAdd(item.Id, item))
                {
                    throw new ArgumentException($"Duplicate item id {item.Id}", nameof(items));
                }

                AddToTypeIndex(item);

                item.OnDestroyed += OnItemDestroyed;
            }
        }

        public Player? Owner { get; }

        public ItemLocation Location { get; }

        public uint LocationId { get; }

        public bool Add(Item item)
        {
            lock (_syncRoot)
            {
                if (!_items.TryAdd(item.Id, item))
                {
                    return false;
                }

                AddToTypeIndex(item);

                item.OnDestroyed += OnItemDestroyed;
                item.ChangeOwner(Owner, Location, LocationId);

                return true;
            }
        }

        public Item? Remove(uint id)
        {
            lock (_syncRoot)
            {
                if (!_items.Remove(id, out var item))
                {
                    return null;
                }

                RemoveFromTypeIndex(item);

                item.OnDestroyed -= OnItemDestroyed;
                item.ChangeOwner(null, ItemLocation.None, 0);

                return item;
            }
        }

        public Item? RemoveOfType(ItemType type)
        {
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

                return item;
            }
        }

        public bool Transfer(uint id, ItemBag toBag)
        {
            if (ReferenceEquals(this, toBag))
            {
                return true;
            }

            var (first, second) = _lockId <= toBag._lockId
                ? (this, toBag)
                : (toBag, this);

            lock (first._syncRoot)
            {
                lock (second._syncRoot)
                {
                    if (!_items.TryGetValue(id, out var item) || toBag._items.ContainsKey(id))
                    {
                        return false;
                    }

                    _items.Remove(id);
                    RemoveFromTypeIndex(item);

                    toBag._items.Add(id, item);
                    toBag.AddToTypeIndex(item);

                    item.OnDestroyed -= OnItemDestroyed;
                    item.OnDestroyed += toBag.OnItemDestroyed;
                    item.ChangeOwner(toBag.Owner, toBag.Location, toBag.LocationId);

                    return true;
                }
            }
        }

        public bool WriteTo(ref PacketItemList itemList)
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
                if (items[i].WriteTo(ref itemList.Items[i]))
                {
                    i++;
                }
            }
            itemList.ItemCount = (uint)i;

            return true;
        }

        private void AddToTypeIndex(Item item)
        {
            lock (_syncRoot)
            {
                if (!_itemsByType.TryGetValue(item.Type, out var byType))
                {
                    byType = [];
                    _itemsByType[item.Type] = byType;
                }

                byType[item.Id] = item;
            }
        }

        private void RemoveFromTypeIndex(Item item)
        {
            lock (_syncRoot)
            {
                if (_itemsByType.TryGetValue(item.Type, out var byType))
                {
                    byType.Remove(item.Id);
                }
            }
        }

        private void OnItemDestroyed(Item item)
        {
            lock (_syncRoot)
            {
                item.OnDestroyed -= OnItemDestroyed;
                _items.Remove(item.Id);
                RemoveFromTypeIndex(item);
            }
        }
    }
}
