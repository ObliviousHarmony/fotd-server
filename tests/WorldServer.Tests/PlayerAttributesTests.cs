using FOMServer.Shared.Core.Enums;
using FOMServer.World.Core.Exceptions;
using FOMServer.World.Core.Players;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.World.Tests
{
    public class PlayerAttributesTests
    {
        [Fact]
        public void GetMetadata_CurrencyRequiresLock()
        {
            var credits = PlayerAttributes.GetMetadata(AttributeType.UniversalCredits);
            Assert.True(credits.LockRequired);
            Assert.Equal(int.MaxValue, credits.Max);
            Assert.Equal(0, credits.Default);
        }

        [Fact]
        public void Constructor_WithoutInitialValues_DefaultsToZero()
        {
            var attrs = CreateAttributes();
            Assert.Equal(0u, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Constructor_WithInitialValues_SetsValues()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.Health] = 500;
            initial[(int)AttributeType.Agility] = 300;

            var attrs = CreateAttributes(initial);

            Assert.Equal(500u, attrs.Get(AttributeType.Health));
            Assert.Equal(300u, attrs.Get(AttributeType.Agility));
        }

        [Fact]
        public void Get_ClampsNegativeToZero()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.Health] = -100;

            var attrs = CreateAttributes(initial);

            Assert.Equal(0u, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Get_ClampsAboveMax()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.Health] = 9999;

            var attrs = CreateAttributes(initial);

            var max = PlayerAttributes.GetMetadata(AttributeType.Health).Max;
            Assert.Equal((uint)max, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Set_StoresValue()
        {
            var attrs = CreateAttributes();
            attrs.Set(AttributeType.Health, 750);

            Assert.Equal(750u, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Set_ClampsToMax()
        {
            var attrs = CreateAttributes();
            attrs.Set(AttributeType.Health, 9999);

            var max = PlayerAttributes.GetMetadata(AttributeType.Health).Max;
            Assert.Equal((uint)max, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Set_ThrowsForLockRequiredAttribute()
        {
            var attrs = CreateAttributes();

            _ = Assert.Throws<InvalidOperationException>(
                () => attrs.Set(AttributeType.UniversalCredits, 100));
        }

        [Fact]
        public void Set_FiresPersistableChange()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.OnPersistableChange += (_, _, _) => { fired = true; return true; };

            attrs.Set(AttributeType.Health, 500);

            Assert.True(fired);
        }

        [Fact]
        public void Change_PositiveDelta()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.Health] = 500;

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, 200);

            Assert.Equal(700u, result);
            Assert.Equal(700u, attrs.Get(AttributeType.Health));
        }

        [Fact]
        public void Change_NegativeDelta()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.Health] = 500;

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, -200);

            Assert.Equal(300u, result);
        }

        [Fact]
        public void Change_ClampsAtZero()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.Health] = 100;

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, -500);

            Assert.Equal(0u, result);
        }

        [Fact]
        public void Change_ClampsAtMax()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.Health] = 900;

            var attrs = CreateAttributes(initial);
            var result = attrs.Change(AttributeType.Health, 500);

            var max = PlayerAttributes.GetMetadata(AttributeType.Health).Max;
            Assert.Equal((uint)max, result);
        }

        [Fact]
        public void Change_ThrowsForLockRequiredAttribute()
        {
            var attrs = CreateAttributes();

            _ = Assert.Throws<InvalidOperationException>(
                () => attrs.Change(AttributeType.Coins, 100));
        }

        [Fact]
        public void Change_FiresPersistableChange()
        {
            var attrs = CreateAttributes();
            var fired = false;
            attrs.OnPersistableChange += (_, _, _) => { fired = true; return true; };

            _ = attrs.Change(AttributeType.Health, 10);

            Assert.True(fired);
        }

        [Fact]
        public void Lock_GetReturnsCurrentValue()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.UniversalCredits] = 1000;

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
        public void Lock_ChangeUpdatesValue()
        {
            var initial = new int[PlayerAttributes.AttributeCount];
            initial[(int)AttributeType.UniversalCredits] = 1000;

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
            attrs.OnPersistableChange += (_, _, _) => { fired = true; return true; };

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
            attrs.OnPersistableChange += (_, _, _) => { fired = true; return true; };

            using (var locked = attrs.Lock(AttributeType.UniversalCredits))
            {
                _ = locked.Get();
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

            _ = Assert.Throws<AttributeDeadlockException>(
                () => attrs.Lock(AttributeType.Coins));
        }

        private static PlayerAttributes CreateAttributes(int[]? initial = null)
        {
            var address = new NetworkAddress { BinaryAddress = 0x0100007F, Port = 7777 };
            var session = new ClientSession(address);
            var player = new Player(1, session, initial);
            return player.Attributes;
        }
    }
}
