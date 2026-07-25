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
        public void PostDeletion_UseValueAndApplyDurabilityLoss_BothThrow()
        {
            var item = TestItemBuilder
                .Create(ItemType.Zanathid5Inflex)
                .WithDurability(10)
                .WithDurabilityLossFactor(100)
                .Build();

            item.Delete();

            Assert.Throws<ItemDeletedException>(() => item.UseValue(1));
            Assert.Throws<ItemDeletedException>(() => item.ApplyDurabilityLoss(1));
        }
    }
}
