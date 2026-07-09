using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;

namespace FOMServer.Shared.Tests.Items
{
    public class ItemContainerTests
    {
        [Fact]
        public void Add_ThenRemove_ReassignsOwnershipBothWays()
        {
            var location = new TestLocation(ItemLocationType.Player, 1);
            var container = new TestItemContainer(location, ItemSlotType.None);

            var item = CreateItem(id: 1);

            Assert.True(container.Add(item));

            var removed = container.Remove(1);

            Assert.Same(item, removed);
        }

        [Fact]
        public void Add_DuplicateId_ReturnsFalseAndDoesNotReplaceExisting()
        {
            var location = new TestLocation(ItemLocationType.Player, 1);
            var container = new TestItemContainer(location, ItemSlotType.None);

            var first = CreateItem(id: 5);
            var second = CreateItem(id: 5);

            Assert.True(container.Add(first));
            Assert.False(container.Add(second));
            Assert.Same(first, container.Remove(5));
        }

        [Fact]
        public void Transfer_MovesItemAndReassignsOwnershipToDestination()
        {
            var locationA = new TestLocation(ItemLocationType.Player, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Player, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var item = CreateItem(id: 7);
            containerA.Add(item);

            var transferred = containerA.Transfer(7, containerB);

            Assert.True(transferred);
            Assert.Null(containerA.Remove(7));
            Assert.NotNull(containerB.Remove(7));
        }

        [Fact]
        public void Transfer_DuplicateIdAtDestination_FailsAndLeavesBothBagsIntact()
        {
            var locationA = new TestLocation(ItemLocationType.Player, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Player, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var itemA = CreateItem(id: 9);
            containerA.Add(itemA);
            var itemB = CreateItem(id: 9);
            containerB.Add(itemB);

            var transferred = containerA.Transfer(9, containerB);

            Assert.False(transferred);
            Assert.Same(itemA, containerA.Remove(9));
        }

        [Fact]
        public void Transfer_ThenDestroy_RemovesFromDestinationBagNotSource()
        {
            var locationA = new TestLocation(ItemLocationType.Player, 1);
            var containerA = new TestItemContainer(locationA, ItemSlotType.None);
            var locationB = new TestLocation(ItemLocationType.Player, 2);
            var containerB = new TestItemContainer(locationB, ItemSlotType.None);

            var item = CreateItem(id: 3, durability: 10, durabilityLossFactor: 100);
            containerA.Add(item);

            containerA.Transfer(3, containerB);

            item.ApplyDurabilityLoss(10);

            Assert.Null(containerB.Remove(3));
        }

        [Fact]
        public void Destroy_AutomaticallyRemovesItemFromBagWithoutExplicitRemove()
        {
            var location = new TestLocation(ItemLocationType.Player, 1);
            var container = new TestItemContainer(location, ItemSlotType.None);

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
                ItemLocationType.Player,
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

            public ItemLocationRef Location => new(_type, _id, null);
        }

        private sealed class TestItemContainer : ItemContainer
        {
            private readonly Dictionary<uint, Item> _items = [];

            public TestItemContainer(IItemLocation location, ItemSlotType slotType) : base(location, slotType)
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
