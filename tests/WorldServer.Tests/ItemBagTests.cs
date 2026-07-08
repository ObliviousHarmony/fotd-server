using FOMServer.Shared.Core.Enums;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Tests
{
    public class ItemBagTests
    {
        [Fact]
        public void Add_ThenRemove_ReassignsOwnershipBothWays()
        {
            var owner = new Player(1, null);
            var bag = new ItemBag(owner, ItemLocation.Inventory, 0, []);
            var item = CreateItem(id: 1);

            Assert.True(bag.Add(item));
            Assert.True(item.BelongsIn(owner, ItemLocation.Inventory));

            var removed = bag.Remove(1);

            Assert.Same(item, removed);
            Assert.True(item.BelongsIn(null, ItemLocation.None));
        }

        [Fact]
        public void Add_DuplicateId_ReturnsFalseAndDoesNotReplaceExisting()
        {
            var owner = new Player(1, null);
            var bag = new ItemBag(owner, ItemLocation.Inventory, 0, []);
            var first = CreateItem(id: 5);
            var second = CreateItem(id: 5);

            Assert.True(bag.Add(first));
            Assert.False(bag.Add(second));
            Assert.Same(first, bag.Remove(5));
        }

        [Fact]
        public void Transfer_MovesItemAndReassignsOwnershipToDestination()
        {
            var ownerA = new Player(1, null);
            var ownerB = new Player(2, null);
            var bagA = new ItemBag(ownerA, ItemLocation.Inventory, 0, []);
            var bagB = new ItemBag(ownerB, ItemLocation.Inventory, 0, []);
            var item = CreateItem(id: 7);
            bagA.Add(item);

            var transferred = bagA.Transfer(7, bagB);

            Assert.True(transferred);
            Assert.True(item.BelongsIn(ownerB, ItemLocation.Inventory));
            Assert.Null(bagA.Remove(7));
            Assert.NotNull(bagB.Remove(7));
        }

        [Fact]
        public void Transfer_DuplicateIdAtDestination_FailsAndLeavesBothBagsIntact()
        {
            var ownerA = new Player(1, null);
            var ownerB = new Player(2, null);
            var bagA = new ItemBag(ownerA, ItemLocation.Inventory, 0, []);
            var bagB = new ItemBag(ownerB, ItemLocation.Inventory, 0, []);
            var itemA = CreateItem(id: 9);
            var itemB = CreateItem(id: 9);
            bagA.Add(itemA);
            bagB.Add(itemB);

            var transferred = bagA.Transfer(9, bagB);

            Assert.False(transferred);
            Assert.Same(itemA, bagA.Remove(9));
        }

        [Fact]
        public void Transfer_ThenDestroy_RemovesFromDestinationBagNotSource()
        {
            // Regression: Transfer must move the OnDestroyed subscription to the
            // destination bag; leaving the source bag subscribed (or unsubscribing
            // neither) would leave stale/duplicate tracking behind.
            var ownerA = new Player(1, null);
            var ownerB = new Player(2, null);
            var bagA = new ItemBag(ownerA, ItemLocation.Inventory, 0, []);
            var bagB = new ItemBag(ownerB, ItemLocation.Inventory, 0, []);
            var item = CreateItem(id: 3, durability: 10, durabilityLossFactor: 100);
            bagA.Add(item);
            bagA.Transfer(3, bagB);

            item.ApplyDurabilityLoss(10);

            Assert.Null(bagB.Remove(3));
        }

        [Fact]
        public void Destroy_AutomaticallyRemovesItemFromBagWithoutExplicitRemove()
        {
            var owner = new Player(1, null);
            var bag = new ItemBag(owner, ItemLocation.Inventory, 0, []);
            var item = CreateItem(id: 4, durability: 10, durabilityLossFactor: 100);
            bag.Add(item);

            item.ApplyDurabilityLoss(10);

            Assert.Null(bag.Remove(4));
        }

        private static Item CreateItem(
            uint id = 1,
            ushort value = 100,
            ushort durability = 100,
            ushort maxDurability = 100,
            byte durabilityLossFactor = 100)
        {
            var placeholderOwner = new Player(id, null);

            return new Item(
                id,
                ItemType.Zanathid5Inflex,
                placeholderOwner,
                ItemLocation.None,
                0,
                value,
                durability,
                maxDurability,
                durabilityLossFactor,
                ItemSecurity.Normal,
                0,
                0,
                ItemQuality.Standard,
                0,
                [0, 0, 0, 0]
            );
        }
    }
}
