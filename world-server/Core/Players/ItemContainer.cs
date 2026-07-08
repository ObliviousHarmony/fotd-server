using FOMServer.Shared.Core.Enums;

namespace FOMServer.World.Core.Players
{
    internal delegate void ItemAddedCallback(Item item);

    internal delegate void ItemRemovedCallback(Item item);

    internal delegate void ItemTransferredCallback(Item item, Player? owner, ItemLocation location, uint locationId);

    internal abstract class ItemContainer
    {
        protected readonly Lock _syncRoot = new();

        private static long s_nextLockId;
        private readonly long _lockId = Interlocked.Increment(ref s_nextLockId);

        public ItemContainer(Player? owner, ItemLocation location, uint locationId)
        {
            Owner = owner;
            Location = location;
            LocationId = locationId;
        }

        public event ItemAddedCallback? OnItemAdded;

        public event ItemRemovedCallback? OnItemRemoved;

        public event ItemTransferredCallback? OnItemTransferred;

        public Player? Owner { get; }

        public ItemLocation Location { get; }

        public uint LocationId { get; }

        public bool Add(Item item)
        {
            lock (_syncRoot)
            {
                if (!Insert(item))
                {
                    return false;
                }

                item.OnDestroyed += OnItemDestroyed;
                item.ChangeOwner(Owner, Location, LocationId);
            }

            OnItemAdded?.Invoke(item);

            return true;
        }

        public Item? Remove(uint id)
        {
            Item? item;
            lock (_syncRoot)
            {
                item = Extract(id);
                if (item is null)
                {
                    return null;
                }

                item.OnDestroyed -= OnItemDestroyed;
                item.ChangeOwner(null, ItemLocation.None, 0);
            }

            OnItemRemoved?.Invoke(item);

            return item;
        }

        public bool Transfer(uint id, ItemContainer to)
        {
            if (ReferenceEquals(this, to))
            {
                return true;
            }

            var (first, second) = _lockId <= to._lockId
                ? (this, to)
                : (to, this);

            Item? item;
            lock (first._syncRoot)
            {
                lock (second._syncRoot)
                {
                    item = Extract(id);
                    if (item is null)
                    {
                        return false;
                    }

                    if (!to.Insert(item))
                    {
                        if (!Insert(item))
                        {
                            throw new InvalidOperationException($"Item {item} lost in container transfer");
                        }

                        return false;
                    }

                    item.OnDestroyed -= OnItemDestroyed;
                    item.OnDestroyed += to.OnItemDestroyed;
                    item.ChangeOwner(to.Owner, to.Location, to.LocationId);
                }
            }

            OnItemTransferred?.Invoke(item, to.Owner, to.Location, to.LocationId);

            return true;
        }

        protected abstract bool Insert(Item item);

        protected abstract Item? Extract(uint id);

        protected abstract void OnItemDestroyed(Item item);
    }
}
