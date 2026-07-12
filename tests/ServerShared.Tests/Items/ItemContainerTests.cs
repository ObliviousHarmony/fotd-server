using System.Collections.ObjectModel;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;

namespace FOMServer.Shared.Tests.Items
{
    public class ItemContainerTests
    {
        [Fact]
        public void Add_ThenRemove_ReassignsOwnershipBothWays()
        {
            var location = new TestLocation(ItemLocationType.Inventory, 1);
            var container = new TestItemContainer(location, ItemSlotType.None);

            var item = CreateItem(id: 1);

            Assert.True(container.TryAdd(item));

            Assert.True(container.TryRemove(out var removed, item.Id));
            Assert.Contains(item, removed);
        }

        [Fact]
        public void Add_DuplicateId_ReturnsFalseAndDoesNotReplaceExisting()
        {
            var location = new TestLocation(ItemLocationType.Inventory, 1);
            var container = new TestItemContainer(location, ItemSlotType.None);

            var first = CreateItem(id: 5);
            var second = CreateItem(id: 5);

            Assert.True(container.TryAdd(first));
            Assert.False(container.TryAdd(second));
        }

        [Fact]
        public void TransferAll_MovesItemAndReassignsOwnershipToDestination()
        {
            var locationA = new TestLocation(ItemLocationType.Inventory, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Inventory, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var item = CreateItem(id: 7);
            var item2 = CreateItem(id: 8);
            containerA.TryAdd(item);
            containerA.TryAdd(item2);

            Assert.True(containerA.TryTransferAll(containerB, out var transferred, out _));

            Assert.Contains(item, transferred);
            Assert.Contains(item2, transferred);
            Assert.False(containerA.TryRemove(out _, item.Id));
            Assert.False(containerA.TryRemove(out _, item2.Id));
            Assert.True(containerB.TryRemove(out _, item.Id));
            Assert.True(containerB.TryRemove(out _, item2.Id));
        }

        [Fact]
        public void Transfer_MovesItemAndReassignsOwnershipToDestination()
        {
            var locationA = new TestLocation(ItemLocationType.Inventory, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Inventory, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var item = CreateItem(id: 7);
            containerA.TryAdd(item);

            Assert.True(containerA.TryTransfer(containerB, out var transferred, out _, item.Id));

            Assert.Contains(item, transferred);
            Assert.False(containerA.TryRemove(out _, item.Id));
            Assert.True(containerB.TryRemove(out _, item.Id));
        }

        [Fact]
        public void Transfer_DuplicateIdAtDestination_FailsAndLeavesBothBagsIntact()
        {
            var locationA = new TestLocation(ItemLocationType.Inventory, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Inventory, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var itemA = CreateItem(id: 9);
            containerA.TryAdd(itemA);
            var itemB = CreateItem(id: 9);
            containerB.TryAdd(itemB);

            Assert.False(containerA.TryTransfer(containerB, out var transferred, out _, itemA.Id));

            Assert.Empty(transferred);
            Assert.True(containerA.TryRemove(out _, itemA.Id));
        }

        [Fact]
        public void Transfer_ThenDestroy_RemovesFromDestinationBag()
        {
            var locationA = new TestLocation(ItemLocationType.Inventory, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Inventory, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var item = CreateItem(id: 3, durability: 10, durabilityLossFactor: 100);
            containerA.TryAdd(item);

            containerA.TryTransfer(containerB, out _, out _, item.Id);

            item.ApplyDurabilityLoss(10);

            Assert.False(containerB.TryRemove(out _, item.Id));
        }

        [Fact]
        public void Transfer_DisplacesExistingItemAtDestination()
        {
            var locationA = new TestLocation(ItemLocationType.Inventory, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Inventory, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None, maxItems: 1);

            var incoming = CreateItem(id: 10);
            var existing = CreateItem(id: 20);

            containerA.TryAdd(incoming);
            containerB.TryAdd(existing);

            Assert.True(containerA.TryTransfer(containerB, out var transferred, out var displaced, incoming.Id));

            Assert.Contains(incoming, transferred);
            Assert.Contains(existing, displaced);

            // Destination now holds the incoming item, and only the incoming item.
            Assert.True(containerB.TryRemove(out var removedFromB, incoming.Id));
            Assert.Contains(incoming, removedFromB);
            Assert.False(containerB.TryRemove(out _, existing.Id));

            // Source now holds the displaced item, and only the displaced item.
            Assert.True(containerA.TryRemove(out var removedFromA, existing.Id));
            Assert.Contains(existing, removedFromA);
            Assert.False(containerA.TryRemove(out _, incoming.Id));
        }

        private static Item CreateItem(
            uint id = 1,
            ushort value = 100,
            ushort durability = 100,
            ushort maxDurability = 100,
            byte durabilityLossFactor = 100)
        {
            return new Item(
                id,
                ItemType.Zanathid5Inflex,
                ItemLocationType.Inventory,
                1,
                ItemSlotType.None,
                value,
                durability,
                maxDurability,
                durabilityLossFactor
            );
        }

        private sealed class TestLocation : IItemLocation
        {
            private readonly ItemLocationType _type;
            private readonly uint _id;

            public TestLocation(ItemLocationType type, uint id)
            {
                _type = type;
                _id = id;
            }

            public ItemLocationRef LocationRef => new(_type, _id, null);

            public ItemContainer? GetItemContainer(ItemSlotType slotType)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ItemContainer> GetItemContainers()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class TestItemContainer : ItemContainer
        {
            private readonly Dictionary<uint, Item> _items = [];
            private readonly uint? _maxItems;

            public TestItemContainer(IItemLocation location, ItemSlotType slotType, uint? maxItems = null) : base(location, slotType)
            {
                _maxItems = maxItems;
            }

            protected override IReadOnlyCollection<Item> GetAllCore()
            {
                return _items.Values;
            }

            protected override IReadOnlyCollection<uint> GetDisplacedIdsFor(params IReadOnlyCollection<uint> idsToInsert)
            {
                if (_maxItems is null || idsToInsert.Count == 0)
                {
                    return [];
                }

                var overflow = _items.Count + idsToInsert.Count - (int)_maxItems.Value;
                if (overflow <= 0)
                {
                    return [];
                }

                // Displace whichever existing ids aren't already part of the incoming batch.
                return [.. _items.Keys.Where(id => !idsToInsert.Contains(id)).Take(overflow)];
            }

            protected override bool CanInsertCore(params IReadOnlyCollection<uint> idsToInsert)
            {
                foreach (var id in idsToInsert)
                {
                    if (_items.ContainsKey(id))
                    {
                        return false;
                    }
                }

                if (_maxItems is not null && _items.Count + idsToInsert.Count > _maxItems.Value)
                {
                    return false;
                }

                return true;
            }

            protected override bool CanInsertCore(IReadOnlyCollection<uint> idsToInsert, IReadOnlyCollection<uint> idsToExtract)
            {
                foreach (var id in idsToInsert)
                {
                    if (_items.ContainsKey(id) && !idsToExtract.Contains(id))
                    {
                        return false;
                    }
                }

                if (_maxItems is not null && _items.Count + idsToInsert.Count - idsToExtract.Count > _maxItems.Value)
                {
                    return false;
                }

                return true;
            }

            protected override bool InsertCore(params IReadOnlyCollection<Item> itemsToInsert)
            {
                List<Item> added = new(itemsToInsert.Count);
                foreach (var item in itemsToInsert)
                {
                    if (!_items.TryAdd(item.Id, item))
                    {
                        return false;
                    }

                    added.Add(item);
                }

                return true;
            }

            protected override bool CanExtractCore(params IReadOnlyCollection<uint> idsToExtract)
            {
                foreach (var id in idsToExtract)
                {
                    if (!_items.ContainsKey(id))
                    {
                        return false;
                    }
                }

                return true;
            }

            protected override IReadOnlyCollection<Item> ExtractCore(params IReadOnlyCollection<uint> idsToExtract)
            {
                List<Item> extracted = [];
                foreach (var id in idsToExtract)
                {
                    if (!_items.Remove(id, out var item))
                    {
                        return [];
                    }

                    extracted.Add(item);
                }

                return extracted;
            }

            protected override void OnItemDestroyed(Item item)
            {
                lock (_syncRoot)
                {
                    item.ItemDestroyed -= OnItemDestroyed;
                    _items.Remove(item.Id);
                }
            }
        }
    }
}
