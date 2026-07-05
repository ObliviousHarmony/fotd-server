using System.Threading;
using FOMServer.Shared.Core.Enums;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Core.Items
{
    internal class ItemBag
    {
        private readonly Lock _syncRoot = new();
        private readonly Dictionary<uint, Item> _items = [];
        private readonly Dictionary<ItemType, Dictionary<uint, Item>> _itemsByType = [];

        public ItemBag(Player? owner, ItemLocation location, uint locationId)
        {
            Owner = owner;
            Location = location;
            LocationId = locationId;
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

                item.ChangeOwner(null, ItemLocation.None, 0);

                return item;
            }
        }

        public Item? RemoveOfType(ItemType type)
        {
            lock (_syncRoot)
            {
                if (!_itemsByType.TryGetValue(type, out var byType))
                {
                    return null;
                }

                foreach (var item in byType.Values)
                {
                    byType.Remove(item.Id);
                    _items.Remove(item.Id);

                    item.ChangeOwner(null, ItemLocation.None, 0);

                    return item;
                }

                return null;
            }
        }

        public bool Transfer(uint id, ItemBag toBag)
        {
            var (first, second) =
                (Location, LocationId).CompareTo((toBag.Location, toBag.LocationId)) <= 0
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

                    item.ChangeOwner(toBag.Owner, toBag.Location, toBag.LocationId);

                    toBag._items.Add(id, item);
                    toBag.AddToTypeIndex(item);

                    return true;
                }
            }
        }

        private void AddToTypeIndex(Item item)
        {
            if (!_itemsByType.TryGetValue(item.Type, out var byType))
            {
                byType = [];
                _itemsByType[item.Type] = byType;
            }

            byType[item.Id] = item;
        }

        private void RemoveFromTypeIndex(Item item)
        {
            if (_itemsByType.TryGetValue(item.Type, out var byType))
            {
                byType.Remove(item.Id);
            }
        }
    }
}
