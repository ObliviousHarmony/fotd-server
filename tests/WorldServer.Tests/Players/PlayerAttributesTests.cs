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
            Assert.Equal(
                PlayerAttributes.GetMetadata(AttributeType.Health).Default,
                attrs.Get(AttributeType.Health)
            );
        }

        [Fact]
        public void Constructor_WithInitialValues_SetsValues()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.Health] = 500,
                [AttributeType.Agility] = 300
            };

            var attrs = CreateAttributes(initial);

            Assert.Equal(500u, attrs.Get(AttributeType.Health));
            Assert.Equal(300u, attrs.Get(AttributeType.Agility));
        }

        [Fact]
        public void Constructor_WithInitialValues_ThrowsWhenOver()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.Health] = 9999,
            };

            Assert.Throws<ArgumentException>(() => CreateAttributes(initial));
        }

        [Fact]
        public void Change_PositiveDelta()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.Health] = 500,
            };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, 200);

            Assert.Equal(700u, result);
            Assert.Equal(700u, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Change_NegativeDelta()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.Health] = 500,
            };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, -200);

            Assert.Equal(300u, result);
        }

        [Fact]
        public void Change_ClampsAtZero()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.Health] = 100,
            };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, -500);

            Assert.Equal(0u, result);
        }

        [Fact]
        public void Change_ClampsAtMax()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.Health] = 900,
            };

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, 500);

            var max = PlayerAttributes.GetMetadata(AttributeType.Health).Max;
            Assert.Equal((uint)max, result);
        }

        [Fact]
        public void Change_ThrowsForLockRequiredAttribute()
        {
            var attrs = CreateAttributes();

            Assert.Throws<InvalidOperationException>(
                () => attrs.Change(AttributeType.Coins, 100));
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
        public void Lock_GetReturnsCurrentValue()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.UniversalCredits] = 1000,
            };

            var attrs = CreateAttributes(initial);

            using var locked = attrs.Lock(AttributeType.UniversalCredits);
            Assert.Equal(1000u, locked.Get());
        }

        [Fact]
        public void Lock_SetUpdatesValue()
        {
            var attrs = CreateAttributes();

            using var locked = attrs.Lock(AttributeType.UniversalCredits);
            locked.Set(500);

            Assert.Equal(500u, locked.Get());
        }

        [Fact]
        public void Lock_SetClampsToMax()
        {
            var attrs = CreateAttributes();

            using var locked = attrs.Lock(AttributeType.Health);
            locked.Set(9999);

            var max = PlayerAttributes.GetMetadata(AttributeType.Health).Max;
            Assert.Equal((uint)max, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Lock_SetFiresPersistableChangeOnDispose()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.PersistableChange += (_, _) => fired = true;

            var locked = attrs.Lock(AttributeType.Health);
            locked.Set(500);

            Assert.False(fired);

            locked.Dispose();

            Assert.True(fired);
        }

        [Fact]
        public void Lock_ChangeUpdatesValue()
        {
            var initial = new Dictionary<AttributeType, uint>
            {
                [AttributeType.UniversalCredits] = 1000,
            };

            var attrs = CreateAttributes(initial);

            using var locked = attrs.Lock(AttributeType.UniversalCredits);
            var result = locked.Change(-300);

            Assert.Equal(700u, result);
            Assert.Equal(700u, locked.Get());
        }

        [Fact]
        public void Lock_DisposeReleasesLock()
        {
            var attrs = CreateAttributes();

            var locked = attrs.Lock(AttributeType.Coins);
            locked.Set(100);
            locked.Dispose();

            // Should be able to lock again without deadlock
            using var locked2 = attrs.Lock(AttributeType.Coins);
            Assert.Equal(100u, locked2.Get());
        }

        [Fact]
        public void Lock_FiresPersistableChangeOnDispose()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.PersistableChange += (_, _) => fired = true;

            using (var locked = attrs.Lock(AttributeType.UniversalCredits))
            {
                locked.Set(500);
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
                locked.Get();
            }

            Assert.False(fired);
        }

        [Fact]
        public void Lock_DoubleDisposeIsNoOp()
        {
            var attrs = CreateAttributes();

            var locked = attrs.Lock(AttributeType.Coins);
            locked.Set(100);
            locked.Dispose();
            locked.Dispose();

            using var locked2 = attrs.Lock(AttributeType.Coins);
            Assert.Equal(100u, locked2.Get());
        }

        [Fact]
        public void Lock_ThrowsDeadlockWhenAlreadyLocked()
        {
            var attrs = CreateAttributes();

            using var locked = attrs.Lock(AttributeType.Coins);

            Assert.Throws<AttributeDeadlockException>(
                () => attrs.Lock(AttributeType.Coins));
        }

        private static PlayerAttributes CreateAttributes(Dictionary<AttributeType, uint>? values = null)
        {
            var builder = TestPlayerBuilder.Create(1);

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
