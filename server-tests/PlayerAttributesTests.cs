using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Persistence;
using FOMServer.World.Core.Exceptions;
using FOMServer.World.Core.Players;

namespace FOMServer.Tests
{
    public class PlayerAttributesTests
    {
        [Fact]
        public void Get_ClampsToRange()
        {
            var attributes = new PlayerAttributes(null!);

            // Set up internal negative value via locked Change
            using (var health = attributes.Lock(PlayerAttribute.Health))
            {
                health.Set(100);
                health.Change(-200); // Would be -100 internally
            }
            Assert.Equal(0u, attributes.Get(PlayerAttribute.Health));

            // Set up value above max via locked Set
            using (var health = attributes.Lock(PlayerAttribute.Health))
            {
                health.Set(5000); // Max is 1000
            }
            Assert.Equal(1000u, attributes.Get(PlayerAttribute.Health));
        }

        [Fact]
        public void Set_ClampsToMax()
        {
            var attributes = new PlayerAttributes(null!);

            attributes.Set(PlayerAttribute.Health, 500);
            Assert.Equal(500u, attributes.Get(PlayerAttribute.Health));

            attributes.Set(PlayerAttribute.Health, 5000); // Max is 1000
            Assert.Equal(1000u, attributes.Get(PlayerAttribute.Health));
        }

        [Fact]
        public void Change_ClampsReturnValue()
        {
            var attributes = new PlayerAttributes(null!);

            // Positive delta
            attributes.Set(PlayerAttribute.Health, 100);
            uint result = attributes.Change(PlayerAttribute.Health, 50);
            Assert.Equal(150u, result);
            Assert.Equal(150u, attributes.Get(PlayerAttribute.Health));

            // Negative delta
            result = attributes.Change(PlayerAttribute.Health, -100);
            Assert.Equal(50u, result);
            Assert.Equal(50u, attributes.Get(PlayerAttribute.Health));

            // Clamps to zero
            result = attributes.Change(PlayerAttribute.Health, -200);
            Assert.Equal(0u, result);

            // Clamps to max
            attributes.Set(PlayerAttribute.Health, 900);
            result = attributes.Change(PlayerAttribute.Health, 200);
            Assert.Equal(1000u, result);

            // Subtract 300 (internal: 1100 - 300 = 800), now below max
            result = attributes.Change(PlayerAttribute.Health, -300);
            Assert.Equal(800u, result);
            Assert.Equal(800u, attributes.Get(PlayerAttribute.Health));
        }

        [Fact]
        public void SetAndChange_ThrowInvalidOperation_WhenLockRequired()
        {
            var attributes = new PlayerAttributes(null!);

            Assert.Throws<InvalidOperationException>(() =>
                attributes.Set(PlayerAttribute.UC, 100));

            Assert.Throws<InvalidOperationException>(() =>
                attributes.Change(PlayerAttribute.UC, 50));
        }

        [Fact]
        public void LockedAttribute_ClampsToRange()
        {
            var attributes = new PlayerAttributes(null!);

            using (var health = attributes.Lock(PlayerAttribute.Health))
            {
                // Set clamps to max
                health.Set(5000);
                Assert.Equal(1000u, health.Get());

                // Change clamps to max
                health.Set(900);
                uint result = health.Change(200);
                Assert.Equal(1000u, result);

                // Change clamps to zero
                health.Set(50);
                result = health.Change(-200);
                Assert.Equal(0u, result);
            }
        }

        [Fact]
        public void LockedAttribute_DisposeReleasesLock()
        {
            var attributes = new PlayerAttributes(null!);

            // First lock and dispose
            var uc = attributes.Lock(PlayerAttribute.UC);
            uc.Dispose();

            // Should be able to re-lock after dispose
            var uc2 = attributes.Lock(PlayerAttribute.UC);

            // Double dispose should be safe
            uc.Dispose();

            uc2.Dispose();
        }

        [Fact]
        public void Lock_ThrowsAttributeDeadlockException_WhenAlreadyLocked()
        {
            var attributes = new PlayerAttributes(null!);
            using var held = attributes.Lock(PlayerAttribute.UC);

            var lockAcquired = false;
            Exception? caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    using var contested = attributes.Lock(PlayerAttribute.UC);
                    lockAcquired = true;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.Start();
            thread.Join();

            Assert.False(lockAcquired);
            Assert.IsType<AttributeDeadlockException>(caughtException);
            Assert.Equal(PlayerAttribute.UC, ((AttributeDeadlockException)caughtException!).Attribute);
        }

        [Fact]
        public void Change_SpinsWhileLocked_ProceedsAfterUnlock()
        {
            var attributes = new PlayerAttributes(null!);
            attributes.Set(PlayerAttribute.Health, 100);

            var lockHandle = attributes.Lock(PlayerAttribute.Health);
            var changeCompleted = false;

            var thread = new Thread(() =>
            {
                attributes.Change(PlayerAttribute.Health, 50);
                changeCompleted = true;
            });

            thread.Start();

            // Give the thread time to start spinning
            Thread.Sleep(50);
            Assert.False(changeCompleted);

            // Release the lock
            lockHandle.Dispose();

            // Wait for the change to complete
            thread.Join(1000);
            Assert.True(changeCompleted);
            Assert.Equal(150u, attributes.Get(PlayerAttribute.Health));
        }

        [Fact]
        public void Change_ConcurrentModifications_SumsCorrectly()
        {
            var attributes = new PlayerAttributes(null!);
            attributes.Set(PlayerAttribute.Health, 0);

            const int threadCount = 10;
            const int incrementsPerThread = 1000;
            var threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < incrementsPerThread; j++)
                    {
                        attributes.Change(PlayerAttribute.Health, 1);
                    }
                });
            }

            foreach (var thread in threads)
                thread.Start();

            foreach (var thread in threads)
                thread.Join();

            // All changes should accumulate, but clamped to max of 1000
            // 10 threads * 1000 increments = 10000 internal, clamped to 1000 on read
            Assert.Equal(1000u, attributes.Get(PlayerAttribute.Health));

            // Verify internal value is actually 10000 by subtracting 9500
            // If internal was only 1000, result would be 0 (clamped from -8500)
            // Since internal is 10000, result is 500 (10000 - 9500 = 500)
            uint result = attributes.Change(PlayerAttribute.Health, -9500);
            Assert.Equal(500u, result);
            Assert.Equal(500u, attributes.Get(PlayerAttribute.Health));
        }

        [Fact]
        public void Lock_DifferentAttributes_NoCrossBlocking()
        {
            var attributes = new PlayerAttributes(null!);
            var ucLocked = false;
            var fcLocked = false;

            using var uc = attributes.Lock(PlayerAttribute.UC);
            ucLocked = true;

            var thread = new Thread(() =>
            {
                using var fc = attributes.Lock(PlayerAttribute.FC);
                fcLocked = true;
            });

            thread.Start();
            thread.Join(1000);

            Assert.True(ucLocked);
            Assert.True(fcLocked);
        }

        [Fact]
        public void OnPersistableChange_FiresForAllModifications()
        {
            var player = new Player { ID = 123 };
            var attributes = new PlayerAttributes(player);
            var callCount = 0;
            IPersistable? lastEntity = null;
            IPersistable? lastAssociation = null;

            attributes.OnPersistableChange += (entity, association, _) =>
            {
                callCount++;
                lastEntity = entity;
                lastAssociation = association;
                return true;
            };

            // Set fires OnPersistableChange
            attributes.Set(PlayerAttribute.Health, 100);
            Assert.Equal(1, callCount);
            Assert.Same(attributes, lastEntity);
            Assert.Same(player, lastAssociation);

            // Change fires OnPersistableChange
            attributes.Change(PlayerAttribute.Health, 50);
            Assert.Equal(2, callCount);

            // LockedAttribute fires OnPersistableChange on Dispose when changed
            using (var health = attributes.Lock(PlayerAttribute.Health))
            {
                health.Set(200);
            }
            Assert.Equal(3, callCount);

            // LockedAttribute does NOT fire OnPersistableChange if no changes made
            using (var health = attributes.Lock(PlayerAttribute.Health))
            {
                _ = health.Get();
            }
            Assert.Equal(3, callCount);
        }
    }
}
