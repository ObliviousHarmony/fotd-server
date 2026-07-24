using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Tests.Factories;

namespace FOMServer.Shared.Tests.Items
{
    public class ItemContainerTests
    {
        [Fact]
        public void Add_ThenRemove_ReassignsOwnershipBothWays()
        {
            var location = new TestLocation(ItemLocationType.Inventory, 1);
            var container = new TestItemContainer(location, ItemSlotType.None);

            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 1)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();

            Assert.True(container.TryAdd(item));

            Assert.True(container.TryRemove(out var removed, item.Id));
            Assert.Contains(item, removed);
        }

        [Fact]
        public void Add_DuplicateId_ReturnsFalseAndDoesNotReplaceExisting()
        {
            var location = new TestLocation(ItemLocationType.Inventory, 1);
            var container = new TestItemContainer(location, ItemSlotType.None);

            var first = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 5)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();
            var second = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 5)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();

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

            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 7)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();
            var item2 = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 8)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();

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

            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 7)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();

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

            var itemA = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 9)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();
            containerA.TryAdd(itemA);
            var itemB = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 9)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();
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

            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 1)
                .WithLocation(ItemLocationType.Inventory, 1)
                .WithDurability(10)
                .Build();

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

            var incoming = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 10)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();
            var existing = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex, 20)
                .WithLocation(ItemLocationType.Inventory, 1)
                .Build();

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

            public TestItemContainer(IItemLocation location, ItemSlotType slotType, uint? maxItems = null)
                : base(location, slotType)
            {
                _maxItems = maxItems;
            }

            protected override IReadOnlyCollection<Item> GetAllCore()
            {
                return _items.Values;
            }

            protected override IReadOnlyCollection<uint> GetDisplacedIdsFor(
                params IReadOnlyCollection<uint> idsToInsert
            )
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

            protected override bool CanInsertCore(
                IReadOnlyCollection<uint> idsToInsert,
                IReadOnlyCollection<uint> idsToExtract
            )
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

            protected override void OnItemDestroyedCore(Item item)
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
