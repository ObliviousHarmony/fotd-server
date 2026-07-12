using System.Net.Sockets;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.World.Core.Exceptions;
using FOMServer.World.Core.Players;
using FOMServer.World.Tests.Factories;

namespace FOMServer.World.Tests.Players
{
    public class PlayerAttributesTests
    {
        [Fact]
        public void GetMetadata_CurrencyRequiresLock()
        {
            var credits = PlayerAttributes.GetMetadata(AttributeType.UniversalCredits);
            Assert.True(credits.LockRequired);
            Assert.Equal(PlayerConstants.AttributeMaxValues[(int)AttributeType.UniversalCredits], credits.Max);
            Assert.Equal(0u, credits.Default);
        }

        [Fact]
        public void Constructor_WithoutInitialValues_UsesDefaults()
        {
            var attrs = CreateAttributes();
            Assert.Equal(PlayerAttributes.GetMetadata(AttributeType.Health).Default, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Constructor_WithInitialValues_SetsValues()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.Health] = 500,
                [AttributeType.Agility] = 300,
            };

            var attrs = CreateAttributes(initial);

            Assert.Equal(500u, attrs.Get(AttributeType.Health));
            Assert.Equal(300u, attrs.Get(AttributeType.Agility));
        }

        [Fact]
        public void Constructor_WithInitialValues_ThrowsWhenOver()
        {
            var initial = new Dictionary<AttributeType, uint> { [AttributeType.Health] = 9999 };

            Assert.Throws<ArgumentException>(() => CreateAttributes(initial));
        }

        [Fact]
        public void Change_PositiveDelta()
        {
            var initial = new Dictionary<AttributeType, uint> { [AttributeType.Health] = 500 };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, 200);

            Assert.Equal(700u, result);
            Assert.Equal(700u, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Change_NegativeDelta()
        {
            var initial = new Dictionary<AttributeType, uint> { [AttributeType.Health] = 500 };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, -200);

            Assert.Equal(300u, result);
        }

        [Fact]
        public void Change_ClampsAtZero()
        {
            var initial = new Dictionary<AttributeType, uint> { [AttributeType.Health] = 100 };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, -500);

            Assert.Equal(0u, result);
        }

        [Fact]
        public void Change_ClampsAtMax()
        {
            var initial = new Dictionary<AttributeType, uint> { [AttributeType.Health] = 900 };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, 500);

            var max = PlayerAttributes.GetMetadata(AttributeType.Health).Max;
            Assert.Equal((uint)max, result);
        }

        [Fact]
        public void Change_ThrowsForLockRequiredAttribute()
        {
            var attrs = CreateAttributes();

            Assert.Throws<InvalidOperationException>(() => attrs.Change(AttributeType.Coins, 100));
        }

        [Fact]
        public void Change_FiresPersistableChange()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.PersistableChange += (_, _) => fired = true;

            attrs.Change(AttributeType.Health, -100);

            Assert.True(fired);
        }

        [Fact]
        public void Change_DoesNotFirePersistableChangeWhenUnchanged()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.PersistableChange += (_, _) => fired = true;

            attrs.Change(AttributeType.Health, 100);

            Assert.False(fired);
        }

        [Fact]
        public void Change_FiresAttributesChangedWithChangedAttribute()
        {
            var attrs = CreateAttributes();

            AttributeType? changed = null;
            attrs.AttributesChanged += (_, mask) =>
            {
                Assert.True(AttributeType.Health.IsMaskSet(mask));
                changed = AttributeType.Health;
            };

            attrs.Change(AttributeType.Health, -100);

            Assert.Equal(AttributeType.Health, changed);
        }

        [Fact]
        public void Lock_Get_ReturnsCurrentValue()
        {
            var initial = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 1000 };

            var attrs = CreateAttributes(initial);

            using var locked = attrs.Lock(AttributeType.UniversalCredits);
            Assert.Equal(1000u, locked.Value);
        }

        [Fact]
        public void Lock_Set_UpdatesValue()
        {
            var attrs = CreateAttributes();

            using var locked = attrs.Lock(AttributeType.UniversalCredits);
            locked.Value = 500;

            Assert.Equal(500u, locked.Value);
        }

        [Fact]
        public void Lock_Set_ClampsToMax()
        {
            var attrs = CreateAttributes();

            using var locked = attrs.Lock(AttributeType.Health);
            locked.Value = 9999;

            var max = PlayerAttributes.GetMetadata(AttributeType.Health).Max;
            Assert.Equal((uint)max, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Lock_Set_FiresPersistableChangeOnDispose()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.PersistableChange += (_, _) => fired = true;

            var locked = attrs.Lock(AttributeType.Health);
            locked.Value = 500;

            Assert.False(fired);

            locked.Dispose();

            Assert.True(fired);
        }

        [Fact]
        public void Lock_Change_UpdatesValue()
        {
            var initial = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 1000 };

            var attrs = CreateAttributes(initial);

            using var locked = attrs.Lock(AttributeType.UniversalCredits);
            var result = locked.Change(-300);

            Assert.Equal(700u, result);
            Assert.Equal(700u, locked.Value);
        }

        [Fact]
        public void Lock_Change_DoesNotFireAttributesChangedUntilDisposed()
        {
            var attrs = CreateAttributes();

            AttributeType? changed = null;
            attrs.AttributesChanged += (_, mask) =>
            {
                Assert.True(AttributeType.Health.IsMaskSet(mask));
                changed = AttributeType.Health;
            };

            var locked = attrs.Lock(AttributeType.Health);
            locked.Value = 500;

            Assert.Null(changed);

            locked.Dispose();

            Assert.Equal(AttributeType.Health, changed);
        }

        [Fact]
        public void Lock_Dispose_ReleasesLock()
        {
            var attrs = CreateAttributes();

            var locked = attrs.Lock(AttributeType.Coins);
            locked.Value = 100;
            locked.Dispose();

            // Should be able to lock again without deadlock
            using var locked2 = attrs.Lock(AttributeType.Coins);
            Assert.Equal(100u, locked2.Value);
        }

        [Fact]
        public void Lock_FiresPersistableChangeOnDispose()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.PersistableChange += (_, _) => fired = true;

            using (var locked = attrs.Lock(AttributeType.UniversalCredits))
            {
                locked.Value = 500;
                Assert.False(fired);
            }

            Assert.True(fired);
        }

        [Fact]
        public void Lock_DoesNotFirePersistableChangeIfUnchanged()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.PersistableChange += (_, _) => fired = true;

            using (var locked = attrs.Lock(AttributeType.UniversalCredits))
            {
                var _ = locked.Value;
            }

            Assert.False(fired);
        }

        [Fact]
        public void Lock_DoubleDisposeThrows()
        {
            var attrs = CreateAttributes();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var locked = attrs.Lock(AttributeType.Coins);
                locked.Value = 100;
                locked.Dispose();
                locked.Dispose();
            });
        }

        [Fact]
        public void Lock_Multiple_Set_IndividualValues()
        {
            var attrs = CreateAttributes();

            using var multiLock = attrs.Lock(AttributeType.Health, AttributeType.Coins);

            multiLock[AttributeType.Health] = 100;
            multiLock[AttributeType.Coins] = 200;

            Assert.Equal(100u, multiLock[AttributeType.Health]);
            Assert.Equal(200u, multiLock[AttributeType.Coins]);
        }

        [Fact]
        public void Lock_Multiple_SingleLockDoesNotThrowForUnlockedAttribute()
        {
            var attrs = CreateAttributes();

            using var multiLock = attrs.Lock(AttributeType.Health, AttributeType.Coins);
            using var locked = attrs.Lock(AttributeType.Stamina);
        }

        [Fact]
        public void Lock_Multiple_SingleLockThrowsWhenHeld()
        {
            var attrs = CreateAttributes();

            Assert.Throws<AttributeDeadlockException>(() =>
            {
                using var multiLock = attrs.Lock(AttributeType.Health, AttributeType.Coins);

                attrs.Lock(AttributeType.Health);
            });
        }

        [Fact]
        public void Lock_ThrowsDeadlockWhenAlreadyLocked()
        {
            var attrs = CreateAttributes();

            using var locked = attrs.Lock(AttributeType.Coins);

            Assert.Throws<AttributeDeadlockException>(() => attrs.Lock(AttributeType.Coins));
        }

        [Fact]
        public void Lock_Multiple_Change_FiresAllChangedAttributes()
        {
            var attrs = CreateAttributes();

            long changedMask = 0;
            attrs.AttributesChanged += (_, mask) => changedMask = mask;

            using (var locked = attrs.Lock(AttributeType.Health, AttributeType.Coins))
            {
                locked[AttributeType.Health] = 500;
                locked[AttributeType.Coins] = 1000;

                Assert.Equal(0, changedMask);
            }

            long expectedMask = 0;
            AttributeType.Health.ApplyToMask(ref expectedMask, true);
            AttributeType.Coins.ApplyToMask(ref expectedMask, true);
            Assert.Equal(expectedMask, changedMask);
        }

        [Fact]
        public void Lock_Multiple_ChangeFiresOnlyOnce()
        {
            var attrs = CreateAttributes();

            var fireCount = 0;
            attrs.AttributesChanged += (_, _) => fireCount++;

            using (var locked = attrs.Lock(AttributeType.Health, AttributeType.Coins))
            {
                locked[AttributeType.Health] = 500;
                locked[AttributeType.Coins] = 1000;
                locked[AttributeType.Health] = 600;
            }

            Assert.Equal(1, fireCount);
        }

        [Fact]
        public void LockPair_GetReturnsCurrentValue()
        {
            var initialA = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 1000 };
            var attrsA = CreateAttributes(initialA);
            var initialB = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 5000 };
            var attrsB = CreateAttributes(initialB);

            using var locks = attrsA.LockPair(attrsB, AttributeType.UniversalCredits);
            var (lockA, lockB) = locks;

            Assert.Equal(1000u, lockA.Value);
            Assert.Equal(5000u, lockB.Value);
        }

        [Fact]
        public void LockPair_OrdersLocksButReturnsInputOrder()
        {
            var initialA = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 1000 };
            var attrsA = CreateAttributes(initialA);
            var initialB = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 5000 };
            var attrsB = CreateAttributes(initialB);

            using var locks = attrsB.LockPair(attrsA, AttributeType.UniversalCredits);
            var (lockB, lockA) = locks;

            Assert.Equal(1000u, lockA.Value);
            Assert.Equal(5000u, lockB.Value);
        }

        [Fact]
        public void LockPair_Multiple_GetReturnsCurrentValue()
        {
            var initialA = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 1000 };
            var attrsA = CreateAttributes(initialA);
            var initialB = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 5000 };
            var attrsB = CreateAttributes(initialB);

            using var locks = attrsA.LockPair(attrsB, AttributeType.UniversalCredits, AttributeType.Coins);
            var (lockA, lockB) = locks;

            Assert.Equal(1000u, lockA[AttributeType.UniversalCredits]);
            Assert.Equal(5000u, lockB[AttributeType.UniversalCredits]);
        }

        [Fact]
        public void LockPair_Multiple_OrdersLocksButReturnsInputOrder()
        {
            var initialA = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 1000 };
            var attrsA = CreateAttributes(initialA);
            var initialB = new Dictionary<AttributeType, uint> { [AttributeType.UniversalCredits] = 5000 };
            var attrsB = CreateAttributes(initialB);

            using var locks = attrsB.LockPair(attrsA, AttributeType.UniversalCredits, AttributeType.Coins);
            var (lockB, lockA) = locks;

            Assert.Equal(1000u, lockA[AttributeType.UniversalCredits]);
            Assert.Equal(5000u, lockB[AttributeType.UniversalCredits]);
        }

        private static PlayerAttributes CreateAttributes(Dictionary<AttributeType, uint>? values = null)
        {
            var builder = TestPlayerBuilder.Create();

            if (values is not null)
            {
                foreach (var (attr, value) in values)
                {
                    builder.WithAttribute(attr, value);
                }
            }

            var player = builder.Build();

            return player.Attributes;
        }
    }
}
