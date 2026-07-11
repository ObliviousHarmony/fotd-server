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
        public void Transfer_MovesItemAndReassignsOwnershipToDestination()
        {
            var locationA = new TestLocation(ItemLocationType.Inventory, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Inventory, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var item = CreateItem(id: 7);
            containerA.TryAdd(item);

            Assert.True(containerA.TryTransfer(containerB, out var transferred, item.Id));

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

            Assert.False(containerA.TryTransfer(containerB, out var transferred, itemA.Id));

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

            containerA.TryTransfer(containerB, out _, item.Id);

            item.ApplyDurabilityLoss(10);

            Assert.False(containerB.TryRemove(out _, item.Id));
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

            public TestItemContainer(IItemLocation location, ItemSlotType slotType) : base(location, slotType)
            {
            }

            public override Item[] GetAll()
            {
                throw new NotImplementedException();
            }

            protected override bool CanInsertCore(uint id)
            {
                return !_items.ContainsKey(id);
            }

            protected override bool InsertCore(Item item)
            {
                return _items.TryAdd(item.Id, item);
            }

            protected override bool CanExtractCore(uint id)
            {
                return _items.ContainsKey(id);
            }

            protected override Item? ExtractCore(uint id)
            {
                _items.Remove(id, out var item);
                return item;
            }

            protected override void OnItemDestroyed(Item item)
            {
                _items.Remove(item.Id);
                item.ItemDestroyed -= OnItemDestroyed;
            }
        }
    }
}
