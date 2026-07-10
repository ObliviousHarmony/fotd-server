using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Shared.Core.Items
{
    public delegate void ItemAddedCallback(ItemContainer container, Item item);

    public delegate void ItemRemovedCallback(ItemContainer container, Item item);

    public delegate void ItemTransferredTo(ItemContainer container, Item item, ItemContainer to);

    public delegate void ItemTransferredFrom(ItemContainer container, Item item, ItemContainer from);

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

        public event ItemAddedCallback? ItemAdded;

        public event ItemRemovedCallback? ItemRemoved;

        public event ItemTransferredTo? ItemTransferredTo;

        public event ItemTransferredFrom? ItemTransferredFrom;

        public IItemLocation Location { get; }

        public ItemSlotType SlotType { get; }

        public abstract Item[] GetAll();

        public bool Add(Item item)
        {
            lock (_syncRoot)
            {
                if (!InsertCore(item))
                {
                    return false;
                }

                item.ItemDestroyed += OnItemDestroyed;
                item.Move(Location, SlotType);
            }

            RaiseOnItemAdded(item);

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

                item.ItemDestroyed -= OnItemDestroyed;
                item.Move(null, ItemSlotType.None);
            }

            RaiseOnItemRemoved(item);

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

                    item.ItemDestroyed -= OnItemDestroyed;
                    item.ItemDestroyed += to.OnItemDestroyed;
                    item.Move(to.Location, to.SlotType);
                }
            }

            ItemTransferredTo?.Invoke(this, item, to);
            to.ItemTransferredFrom?.Invoke(to, item, this);

            return item;
        }

        protected void RaiseOnItemAdded(Item item)
        {
            ItemAdded?.Invoke(this, item);
        }

        protected void RaiseOnItemRemoved(Item item)
        {
            ItemRemoved?.Invoke(this, item);
        }

        protected abstract Item? GetCore(uint id);

        protected abstract bool InsertCore(Item item);

        protected abstract Item? ExtractCore(uint id);

        protected abstract void OnItemDestroyed(Item item);
    }
}
