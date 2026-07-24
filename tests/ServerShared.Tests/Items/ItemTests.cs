using System.Threading;
using System.Threading.Tasks;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Tests.Factories;

namespace FOMServer.Shared.Tests.Items
{
    public class ItemTests
    {
        [Fact]
        public void UseValue_ClampsToAvailableValue_NeverUnderflows()
        {
            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex)
                .WithValue(10)
                .WithDurabilityLossFactor(0)
                .Build();

            var consumed = item.UseValue(9999);

            Assert.Equal(10, consumed);
        }

        [Fact]
        public void UseValue_WithDurability_DestroysItemWhenDurabilityReachesZero()
        {
            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex)
                .WithValue(100)
                .WithDurability(10)
                .WithDurabilityLossFactor(100)
                .Build();

            var destroyedCount = 0;
            item.ItemDestroyed += _ => destroyedCount++;

            item.UseValue(10, true);

            Assert.Equal(1, destroyedCount);
        }

        [Fact]
        public void ApplyDurabilityLoss_DurabilityReachesZero_DestroysItem()
        {
            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex)
                .WithDurability(20)
                .WithDurabilityLossFactor(100)
                .Build();

            var destroyedCount = 0;
            item.ItemDestroyed += _ => destroyedCount++;

            item.ApplyDurabilityLoss(20);

            Assert.Equal(1, destroyedCount);
        }

        [Fact]
        public void ApplyDurabilityLoss_LargeLossFactor_DestroysInsteadOfWrappingAround()
        {
            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex)
                .WithDurability(50)
                .WithDurabilityLossFactor(250)
                .Build();

            var destroyedCount = 0;
            item.ItemDestroyed += _ => destroyedCount++;

            item.ApplyDurabilityLoss(60000);

            Assert.Equal(1, destroyedCount);
        }

        [Fact]
        public void PostDestruction_UseValueAndApplyDurabilityLoss_BothThrow()
        {
            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex)
                .WithDurability(10)
                .WithDurabilityLossFactor(100)
                .Build();

            item.ApplyDurabilityLoss(10);

            Assert.Throws<ItemDestroyedException>(() => item.UseValue(1));
            Assert.Throws<ItemDestroyedException>(() => item.ApplyDurabilityLoss(1));
        }
    }
}
