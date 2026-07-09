using FOMServer.Shared.Core.Enums;

namespace FOMServer.Shared.Core.Items
{
    public delegate void ItemAddedCallback(Item item);

    public delegate void ItemRemovedCallback(Item item);

    public delegate void ItemTransferredCallback(Item item, IItemLocation location);

    public abstract class ItemContainer
    {
        protected readonly Lock _syncRoot = new();

        private static long s_nextLockId;
        private readonly long _lockId = Interlocked.Increment(ref s_nextLockId);

        public ItemContainer(IItemLocation location, ItemSlotType slotType)
        {
            Location = location;
            SlotType = slotType;
        }

        public event ItemAddedCallback? OnItemAdded;

        public event ItemRemovedCallback? OnItemRemoved;

        public event ItemTransferredCallback? OnItemTransferred;

        public IItemLocation Location { get; }

        public ItemSlotType SlotType { get; }

        public bool Add(Item item)
        {
            lock (_syncRoot)
            {
                if (!InsertCore(item))
                {
                    return false;
                }

                item.OnDestroyed += OnItemDestroyed;
                item.Move(Location, SlotType);
            }

            OnItemAdded?.Invoke(item);

            return true;
        }

        public Item? Remove(uint id)
        {
            Item? item;
            lock (_syncRoot)
            {
                item = ExtractCore(id);
                if (item is null)
                {
                    return null;
                }

                item.OnDestroyed -= OnItemDestroyed;
                item.Move(null, ItemSlotType.None);
            }

            OnItemRemoved?.Invoke(item);

            return item;
        }

        public Item? Transfer(uint id, ItemContainer to)
        {
            if (ReferenceEquals(this, to))
            {
                lock (_syncRoot)
                {
                    return GetCore(id);
                }
            }

            var (first, second) = _lockId <= to._lockId
                ? (this, to)
                : (to, this);

            Item? item;
            lock (first._syncRoot)
            {
                lock (second._syncRoot)
                {
                    item = ExtractCore(id);
                    if (item is null)
                    {
                        return null;
                    }

                    if (!to.InsertCore(item))
                    {
                        if (!InsertCore(item))
                        {
                            throw new InvalidOperationException($"Item {item} lost in container transfer");
                        }

                        return null;
                    }

                    item.OnDestroyed -= OnItemDestroyed;
                    item.OnDestroyed += to.OnItemDestroyed;
                    item.Move(to.Location, to.SlotType);
                }
            }

            OnItemTransferred?.Invoke(item, to.Location);

            return item;
        }

        protected void RaiseOnItemAdded(Item item)
        {
            OnItemAdded?.Invoke(item);
        }

        protected void RaiseOnItemRemoved(Item item)
        {
            OnItemRemoved?.Invoke(item);
        }

        protected abstract Item? GetCore(uint id);

        protected abstract bool InsertCore(Item item);

        protected abstract Item? ExtractCore(uint id);

        protected abstract void OnItemDestroyed(Item item);
    }
}
