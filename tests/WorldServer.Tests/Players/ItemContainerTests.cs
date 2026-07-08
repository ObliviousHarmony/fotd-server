using FOMServer.Shared.Core.Enums;
using FOMServer.World.Core.Players;
using FOMServer.World.Tests.Factories;

namespace FOMServer.World.Tests.Players
{
    public class ItemContainerTests
    {
        [Fact]
        public void Add_ThenRemove_ReassignsOwnershipBothWays()
        {
            var owner = TestPlayerBuilder.Create(1).Build();
            var container = new TestItemContainer(owner, ItemLocation.Inventory, 0);
            var item = CreateItem(id: 1);

            Assert.True(container.Add(item));
            Assert.True(item.BelongsIn(ItemLocation.Inventory));

            var removed = container.Remove(1);

            Assert.Same(item, removed);
            Assert.True(item.BelongsIn(ItemLocation.None));
        }

        [Fact]
        public void Add_DuplicateId_ReturnsFalseAndDoesNotReplaceExisting()
        {
            var owner = TestPlayerBuilder.Create(1).Build();
            var container = new TestItemContainer(owner, ItemLocation.Inventory, 0);
            var first = CreateItem(id: 5);
            var second = CreateItem(id: 5);

            Assert.True(container.Add(first));
            Assert.False(container.Add(second));
            Assert.Same(first, container.Remove(5));
        }

        [Fact]
        public void Transfer_MovesItemAndReassignsOwnershipToDestination()
        {
            var ownerA = TestPlayerBuilder.Create(1).Build();
            var ownerB = TestPlayerBuilder.Create(2).Build();
            var containerA = new TestItemContainer(ownerA, ItemLocation.Inventory, 0);
            var containerB = new TestItemContainer(ownerB, ItemLocation.Inventory, 0);
            var item = CreateItem(id: 7);
            containerA.Add(item);

            var transferred = containerA.Transfer(7, containerB);

            Assert.True(transferred);
            Assert.True(item.BelongsTo(ownerB));
            Assert.True(item.BelongsIn(ItemLocation.Inventory));
            Assert.Null(containerA.Remove(7));
            Assert.NotNull(containerB.Remove(7));
        }

        [Fact]
        public void Transfer_DuplicateIdAtDestination_FailsAndLeavesBothBagsIntact()
        {
            var ownerA = TestPlayerBuilder.Create(1).Build();
            var ownerB = TestPlayerBuilder.Create(2).Build();
            var containerA = new TestItemContainer(ownerA, ItemLocation.Inventory, 0);
            var containerB = new TestItemContainer(ownerB, ItemLocation.Inventory, 0);
            var itemA = CreateItem(id: 9);
            var itemB = CreateItem(id: 9);
            containerA.Add(itemA);
            containerB.Add(itemB);

            var transferred = containerA.Transfer(9, containerB);

            Assert.False(transferred);
            Assert.Same(itemA, containerA.Remove(9));
        }

        [Fact]
        public void Transfer_ThenDestroy_RemovesFromDestinationBagNotSource()
        {
            var ownerA = TestPlayerBuilder.Create(1).Build();
            var ownerB = TestPlayerBuilder.Create(2).Build();
            var containerA = new TestItemContainer(ownerA, ItemLocation.Inventory, 0);
            var containerB = new TestItemContainer(ownerB, ItemLocation.Inventory, 0);
            var item = CreateItem(id: 3, durability: 10, durabilityLossFactor: 100);
            containerA.Add(item);
            containerA.Transfer(3, containerB);

            item.ApplyDurabilityLoss(10);

            Assert.Null(containerB.Remove(3));
        }

        [Fact]
        public void Destroy_AutomaticallyRemovesItemFromBagWithoutExplicitRemove()
        {
            var owner = TestPlayerBuilder.Create(1).Build();
            var container = new TestItemContainer(owner, ItemLocation.Inventory, 0);
            var item = CreateItem(id: 4, durability: 10, durabilityLossFactor: 100);
            container.Add(item);

            item.ApplyDurabilityLoss(10);

            Assert.Null(container.Remove(4));
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
                1,
                ItemLocation.None,
                0,
                value,
                durability,
                maxDurability,
                durabilityLossFactor
            );
        }

        private sealed class TestItemContainer : ItemContainer
        {
            private readonly Dictionary<uint, Item> _items = [];

            public TestItemContainer(Player? owner, ItemLocation location, uint locationId)
                : base(owner, location, locationId)
            {
            }

            protected override bool Insert(Item item)
            {
                return _items.TryAdd(item.Id, item);
            }

            protected override Item? Extract(uint id)
            {
                _items.Remove(id, out var item);
                return item;
            }

            protected override void OnItemDestroyed(Item item)
            {
                _items.Remove(item.Id);
                item.OnDestroyed -= OnItemDestroyed;
            }
        }
    }
}
