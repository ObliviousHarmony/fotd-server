using System.Collections.Immutable;
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

        public IReadOnlyCollection<Item> GetAll()
        {
            lock (_syncRoot)
            {
                // Always create a new collection so we protect ourselves
                // from accidentally returning the backing array.
                return [.. GetAllCore()];
            }
        }

        public bool TryAdd(params IReadOnlyCollection<Item> items)
        {
            if (items.Count == 0)
            {
                return true;
            }

            var ids = new uint[items.Count];
            var i = 0;
            foreach (var item in items)
            {
                ids[i++] = item.Id;
            }

            lock (_syncRoot)
            {
                if (!CanInsertCore(ids))
                {
                    return false;
                }

                if (!InsertCore(items))
                {
                    throw new InvalidOperationException($"Item(s) {string.Join(", ", ids)} could not be inserted");
                }

                foreach (var item in items)
                {
                    item.ItemDestroyed += OnItemDestroyed;
                    item.ChangeLocation(Location, SlotType);
                }
            }

            return true;
        }

        public bool TryRemove(out List<Item> removed, params IReadOnlyCollection<uint> ids)
        {
            removed = new(ids.Count);

            if (ids.Count == 0)
            {
                return true;
            }

            lock (_syncRoot)
            {
                if (!CanExtractCore(ids))
                {
                    return false;
                }

                var extracted = ExtractCore(ids);
                if (extracted.Count != ids.Count)
                {
                    throw new InvalidOperationException($"Item(s) {string.Join(", ", ids)} could not be extracted");
                }

                foreach (var item in extracted)
                {
                    item.ItemDestroyed -= OnItemDestroyed;
                    item.ChangeLocation(null, ItemSlotType.None);

                    removed.Add(item);
                }
            }

            return true;
        }

        public bool TryTransferAll(ItemContainer to, out List<Item> transferred, out List<Item> displaced)
        {
            uint[] ids;
            lock (_syncRoot)
            {
                var items = GetAllCore();
                if (items.Count == 0)
                {
                    transferred = [];
                    displaced = [];
                    return true;
                }

                ids = new uint[items.Count];
                var i = 0;
                foreach (var item in items)
                {
                    ids[i++] = item.Id;
                }
            }

            return TryTransfer(to, out transferred, out displaced, ids);
        }

        public bool TryTransfer(ItemContainer to, out List<Item> transferred, out List<Item> displaced, params IReadOnlyCollection<uint> ids)
        {
            transferred = new(ids.Count);
            displaced = [];

            if (ids.Count == 0)
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
                    if (!CanExtractCore(ids))
                    {
                        return false;
                    }

                    var idsToDisplace = to.GetDisplacedIdsFor(ids);
                    if (idsToDisplace.Count > 0 && !CanInsertCore(idsToDisplace, ids))
                    {
                        return false;
                    }

                    if (!to.CanInsertCore(ids, idsToDisplace))
                    {
                        return false;
                    }

                    var displacedItems = to.ExtractCore(idsToDisplace);
                    if (displacedItems.Count != idsToDisplace.Count)
                    {
                        throw new InvalidOperationException($"Item(s) {string.Join(", ", idsToDisplace)} could not be extracted");
                    }

                    var extractedItems = ExtractCore(ids);
                    if (extractedItems.Count != ids.Count)
                    {
                        throw new InvalidOperationException($"Item(s) {string.Join(", ", ids)} could not be extracted");
                    }

                    if (!to.InsertCore(extractedItems))
                    {
                        throw new InvalidOperationException($"Item(s) {string.Join(", ", ids)} could not be inserted");
                    }

                    foreach (var item in extractedItems)
                    {
                        item.ItemDestroyed -= OnItemDestroyed;
                        item.ItemDestroyed += to.OnItemDestroyed;
                        item.ChangeLocation(to.Location, to.SlotType);
                        transferred.Add(item);
                    }

                    if (displacedItems.Count > 0)
                    {
                        if (!InsertCore(displacedItems))
                        {
                            throw new InvalidOperationException($"Item(s) {string.Join(", ", idsToDisplace)} could not be insert");
                        }

                        foreach (var item in displacedItems)
                        {
                            item.ItemDestroyed -= to.OnItemDestroyed;
                            item.ItemDestroyed += OnItemDestroyed;
                            item.ChangeLocation(Location, SlotType);
                            displaced.Add(item);
                        }
                    }
                }
            }

            return true;
        }

        protected abstract IReadOnlyCollection<Item> GetAllCore();

        protected abstract IReadOnlyCollection<uint> GetDisplacedIdsFor(params IReadOnlyCollection<uint> idsToInsert);

        protected abstract bool CanInsertCore(params IReadOnlyCollection<uint> idsToInsert);

        protected abstract bool CanInsertCore(IReadOnlyCollection<uint> idsToInsert, IReadOnlyCollection<uint> idsToExtract);

        protected abstract bool InsertCore(params IReadOnlyCollection<Item> itemsToInsert);

        protected abstract bool CanExtractCore(params IReadOnlyCollection<uint> idsToExtract);

        protected abstract IReadOnlyCollection<Item> ExtractCore(params IReadOnlyCollection<uint> idsToExtract);

        protected abstract void OnItemDestroyed(Item item);
    }
}
