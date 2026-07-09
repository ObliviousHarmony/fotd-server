using System.Threading;
using System.Threading.Tasks;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;

namespace FOMServer.Shared.Tests.Items
{
    public class ItemTests
    {
        [Fact]
        public void UseValue_ClampsToAvailableValue_NeverUnderflows()
        {
            var item = CreateItem(value: 10, durabilityLossFactor: 0);

            var consumed = item.UseValue(9999);

            Assert.Equal(10, consumed);
        }

        [Fact]
        public void UseValue_WithDurability_DestroysItemWhenDurabilityReachesZero()
        {
            var item = CreateItem(value: 100, durability: 10, durabilityLossFactor: 100);
            var destroyedCount = 0;
            item.OnDestroyed += _ => destroyedCount++;

            item.UseValue(10, true);

            Assert.Equal(1, destroyedCount);
        }

        [Fact]
        public void ApplyDurabilityLoss_DurabilityReachesZero_DestroysItem()
        {
            var item = CreateItem(durability: 20, durabilityLossFactor: 100);
            var destroyedCount = 0;
            item.OnDestroyed += _ => destroyedCount++;

            item.ApplyDurabilityLoss(20);

            Assert.Equal(1, destroyedCount);
        }

        [Fact]
        public void ApplyDurabilityLoss_LargeLossFactor_DestroysInsteadOfWrappingAround()
        {
            var item = CreateItem(durability: 50, durabilityLossFactor: 250);
            var destroyedCount = 0;
            item.OnDestroyed += _ => destroyedCount++;

            item.ApplyDurabilityLoss(60000);

            Assert.Equal(1, destroyedCount);
        }

        [Fact]
        public void PostDestruction_UseValueAndApplyDurabilityLoss_BothThrow()
        {
            var item = CreateItem(durability: 10, durabilityLossFactor: 100);

            item.ApplyDurabilityLoss(10);

            Assert.Throws<ItemDestroyedException>(() => item.UseValue(1));
            Assert.Throws<ItemDestroyedException>(() => item.ApplyDurabilityLoss(1));
        }

        private static Item CreateItem(
            ushort value = 100,
            ushort durability = 100,
            ushort maxDurability = 100,
            byte durabilityLossFactor = 100)
        {
            return new Item(
                1,
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
    }
}
