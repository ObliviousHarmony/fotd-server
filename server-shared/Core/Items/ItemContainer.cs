using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;

namespace FOMServer.Shared.Core.Items
{
    public delegate void ItemsAddedToContainerHandler(ItemContainer container, IReadOnlyCollection<Item> items);
    public delegate void ItemsRemovedFromContainerHandler(ItemContainer container, IReadOnlyCollection<Item> items);
    public delegate void ItemsTransferredBetweenContainers(
        ItemContainer fromContainer,
        ItemContainer toContainer,
        IReadOnlyCollection<Item> items
    );
    public delegate void ItemDestroyedInContainerHandler(ItemContainer container, Item item);

    public abstract class ItemContainer : IPersistableProvider
    {
        protected readonly Lock _syncRoot = new();

        private static long s_nextLockId;
        private readonly long _lockId = Interlocked.Increment(ref s_nextLockId);

        public ItemContainer(IItemLocation location, ItemSlotType slotType)
        {
            Location = location;
            SlotType = slotType;
        }

        public event ItemsAddedToContainerHandler? ItemsAdded;
        public event ItemsRemovedFromContainerHandler? ItemsRemoved;
        public event ItemsTransferredBetweenContainers? ItemsTransferred;
        public event ItemDestroyedInContainerHandler? ItemDestroyed;

        public IItemLocation Location { get; }

        public ItemSlotType SlotType { get; }

        public void CollectPersistables(ICollection<IPersistable> destination)
        {
            lock (_syncRoot)
            {
                foreach (var item in GetAllCore())
                {
                    destination.Add(item);
                }
            }
        }

        public bool TryGetItemSnapshot(uint id, out ItemSnapshot snapshot)
        {
            Item? item;
            lock (_syncRoot)
            {
                item = GetCore(id);
            }

            if (item is null)
            {
                snapshot = default;
                return false;
            }

            snapshot = item.ToSnapshot();
            return true;
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
                    throw new InvalidOperationException(
                        $"ItemInterop(s) {string.Join(", ", ids)} could not be inserted"
                    );
                }

                foreach (var item in items)
                {
                    item.ItemDestroyed += OnItemDestroyed;
                    item.ChangeLocation(Location, SlotType);
                }
            }

            ItemsAdded?.Invoke(this, items);

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
                    throw new InvalidOperationException(
                        $"ItemInterop(s) {string.Join(", ", ids)} could not be extracted"
                    );
                }

                foreach (var item in extracted)
                {
                    item.ItemDestroyed -= OnItemDestroyed;
                    item.ChangeLocation(null, ItemSlotType.None);

                    removed.Add(item);
                }
            }

            ItemsRemoved?.Invoke(this, removed);

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

        public bool TryTransfer(
            ItemContainer to,
            out List<Item> transferred,
            out List<Item> displaced,
            params IReadOnlyCollection<uint> ids
        )
        {
            transferred = new(ids.Count);
            displaced = [];

            if (ids.Count == 0)
            {
                return true;
            }

            var (first, second) = _lockId <= to._lockId ? (this, to) : (to, this);

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
                        throw new InvalidOperationException(
                            $"ItemInterop(s) {string.Join(", ", idsToDisplace)} could not be extracted"
                        );
                    }

                    var extractedItems = ExtractCore(ids);
                    if (extractedItems.Count != ids.Count)
                    {
                        throw new InvalidOperationException(
                            $"ItemInterop(s) {string.Join(", ", ids)} could not be extracted"
                        );
                    }

                    if (!to.InsertCore(extractedItems))
                    {
                        throw new InvalidOperationException(
                            $"ItemInterop(s) {string.Join(", ", ids)} could not be inserted"
                        );
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
                            throw new InvalidOperationException(
                                $"ItemInterop(s) {string.Join(", ", idsToDisplace)} could not be insert"
                            );
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

            ItemsTransferred?.Invoke(this, to, transferred);
            to.ItemsTransferred?.Invoke(this, to, transferred);
            if (displaced.Count > 0)
            {
                ItemsTransferred?.Invoke(to, this, displaced);
                to.ItemsTransferred?.Invoke(to, this, displaced);
            }

            return true;
        }

        protected void OnItemDestroyed(Item item)
        {
            OnItemDestroyedCore(item);

            ItemDestroyed?.Invoke(this, item);
        }

        protected abstract Item? GetCore(uint id);

        protected abstract IReadOnlyCollection<Item> GetAllCore();

        protected abstract IReadOnlyCollection<uint> GetDisplacedIdsFor(params IReadOnlyCollection<uint> idsToInsert);

        protected abstract bool CanInsertCore(params IReadOnlyCollection<uint> idsToInsert);

        protected abstract bool CanInsertCore(
            IReadOnlyCollection<uint> idsToInsert,
            IReadOnlyCollection<uint> idsToExtract
        );

        protected abstract bool InsertCore(params IReadOnlyCollection<Item> itemsToInsert);

        protected abstract bool CanExtractCore(params IReadOnlyCollection<uint> idsToExtract);

        protected abstract IReadOnlyCollection<Item> ExtractCore(params IReadOnlyCollection<uint> idsToExtract);

        protected abstract void OnItemDestroyedCore(Item item);
    }
}
