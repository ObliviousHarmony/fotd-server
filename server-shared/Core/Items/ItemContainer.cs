using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Shared.Core.Items
{
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

        public IItemLocation Location { get; }

        public ItemSlotType SlotType { get; }

        public abstract Item[] GetAll();

        public bool TryAdd(params IReadOnlyCollection<Item> items)
        {
            if (items.Count == 0)
            {
                return true;
            }

            lock (_syncRoot)
            {
                foreach (var item in items)
                {
                    if (!CanInsertCore(item.Id))
                    {
                        return false;
                    }
                }

                foreach (var item in items)
                {
                    if (!InsertCore(item))
                    {
                        throw new InvalidOperationException($"Item {item.Id} could not be added");
                    }

                    item.ItemDestroyed += OnItemDestroyed;
                    item.ChangeLocation(Location, SlotType);
                }
            }

            return true;
        }

        public bool TryRemove(out List<Item> removed, params ReadOnlySpan<uint> ids)
        {
            removed = new(ids.Length);

            if (ids.Length == 0)
            {
                return true;
            }

            lock (_syncRoot)
            {
                foreach (var id in ids)
                {
                    if (!CanExtractCore(id))
                    {
                        return false;
                    }
                }

                foreach (var id in ids)
                {
                    var item = ExtractCore(id) ?? throw new InvalidOperationException($"Item {id} could not be removed");
 
                    item.ItemDestroyed -= OnItemDestroyed;
                    item.ChangeLocation(null, ItemSlotType.None);

                    removed.Add(item);
                }
            }

            return true;
        }

        public bool TryTransfer(ItemContainer to, out List<Item> transferred, params ReadOnlySpan<uint> ids)
        {
            transferred = new(ids.Length);

            if (ids.Length == 0)
            {
                return true;
            }

            var (first, second) = _lockId <= to._lockId
                ? (this, to)
                : (to, this);

            lock (first._syncRoot)
            {
                lock (second._syncRoot)
                {
                    foreach (var id in ids)
                    {
                        if (!CanExtractCore(id))
                        {
                            return false;
                        }

                        if (!to.CanInsertCore(id))
                        {
                            return false;
                        }
                    }

                    foreach (var id in ids)
                    {
                        var item = ExtractCore(id) ?? throw new InvalidOperationException($"Item {id} could not be extracted");

                        if (!to.InsertCore(item))
                        {
                           throw new InvalidOperationException($"Item {item} lost in container transfer");
                        }

                        item.ItemDestroyed -= OnItemDestroyed;
                        item.ItemDestroyed += to.OnItemDestroyed;
                        item.ChangeLocation(to.Location, to.SlotType);

                        transferred.Add(item);
                    }
                }
            }

            return true;
        }

        protected abstract bool CanInsertCore(uint id);

        protected abstract bool InsertCore(Item item);

        protected abstract bool CanExtractCore(uint id);

        protected abstract Item? ExtractCore(uint id);

        protected abstract void OnItemDestroyed(Item item);
    }
}
