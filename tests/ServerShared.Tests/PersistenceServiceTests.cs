using FOMServer.Shared.Application.Persistence;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Tests
{
    public class PersistenceServiceTest : IDisposable
    {
        private readonly Mock<IShutdownManager> _shutdownManager;
        private readonly Mock<ILogger<PersistenceService>> _logger;
        private readonly TestPersistenceHandler _handler;
        private readonly CancellationTokenSource _cts;

        public PersistenceServiceTest()
        {
            _cts = new CancellationTokenSource();
            _shutdownManager = new Mock<IShutdownManager>();
            _shutdownManager.Setup(s => s.Token).Returns(_cts.Token);
            _shutdownManager.Setup(s => s.TrackTask(It.IsAny<Task>()));

            _logger = new Mock<ILogger<PersistenceService>>();
            _handler = new TestPersistenceHandler();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task MarkChanged_EntityIsPersisted()
        {
            var service = CreateService();
            var entity = new TestEntity();

            service.Register(entity);

            entity.MarkChanged();

            // Wait for persistence loop to process
            await Task.Delay(200);

            Assert.Contains(entity, _handler.PersistedEntities);
        }

        [Fact]
        public async Task WaitForPersistence_CallbackFiresAfterPersist()
        {
            var service = CreateService();
            var entity = new TestEntity();
            var callbackFired = new TaskCompletionSource();

            service.Register(entity);

            entity.MarkChanged();
            service.WaitForPersistence(entity, callbackFired.SetResult);

            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));

            Assert.Equal(callbackFired.Task, completed);
            Assert.Contains(entity, _handler.PersistedEntities);
        }

        [Fact]
        public async Task WaitForPersistence_WaitsForAssociations()
        {
            var service = CreateService();
            var player = new TestEntity();
            var item = new TestEntity();
            var callbackFired = new TaskCompletionSource();

            service.Register(player);
            service.Register(item);

            // Item changes and registers player as an association
            // (player must wait for item to persist before its wait completes)
            item.MarkChanged(player);

            service.WaitForPersistence(player, callbackFired.SetResult);

            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));

            Assert.Equal(callbackFired.Task, completed);
            Assert.Contains(item, _handler.PersistedEntities);
            Assert.Contains(player, _handler.PersistedEntities);
        }

        [Fact]
        public async Task WaitForPersistence_WaitsForAdditionalAssociations()
        {
            var service = CreateService();
            var player = new TestEntity();
            var item1 = new TestEntity();
            var item2 = new TestEntity();
            var callbackFired = new TaskCompletionSource();

            service.Register(player);
            service.Register(item1);
            service.Register(item2);

            // Item1 changes with null primary association but player in additional associations
            item1.MarkChanged(null, player);

            // Item2 changes with both primary and additional associations
            item2.MarkChanged(player, item1);

            service.WaitForPersistence(player, callbackFired.SetResult);

            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));

            Assert.Equal(callbackFired.Task, completed);
            Assert.Contains(item1, _handler.PersistedEntities);
            Assert.Contains(item2, _handler.PersistedEntities);
            Assert.Contains(player, _handler.PersistedEntities);
        }

        [Fact]
        public async Task WaitForPersistence_BlocksUntilAssociationPersists()
        {
            var service = CreateService();
            var player = new TestEntity();
            var item = new TestEntity();
            var callbackFired = new TaskCompletionSource();

            // Block item persistence until we release it
            var blockItem = new TaskCompletionSource();
            _handler.BlockUntil = blockItem;

            service.Register(player);
            service.Register(item);

            // Item changes and registers player as an association
            item.MarkChanged(player);

            service.WaitForPersistence(player, callbackFired.SetResult);

            // Wait a bit - callback should NOT have fired yet
            await Task.Delay(200);
            Assert.False(callbackFired.Task.IsCompleted, "Callback should not fire until item is persisted");

            // Release the block
            blockItem.SetResult();

            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));
            Assert.Equal(callbackFired.Task, completed);
        }

        [Fact]
        public async Task WaitForPersistence_MultipleWaitsOnSameEntity_AllCallbacksFire()
        {
            var service = CreateService();
            var entity = new TestEntity();
            var firstFired = new TaskCompletionSource();
            var secondFired = new TaskCompletionSource();

            // Block item persistence until both callbacks have been registered.
            var blockPersist = new TaskCompletionSource();
            _handler.BlockUntil = blockPersist;

            service.Register(entity);

            entity.MarkChanged();
            service.WaitForPersistence(entity, firstFired.SetResult);
            service.WaitForPersistence(entity, secondFired.SetResult);

            await Task.Delay(150);
            Assert.False(firstFired.Task.IsCompleted, "First wait must not fire before the flush");
            Assert.False(secondFired.Task.IsCompleted, "Second wait must not fire before the flush");

            // Release the lock
            blockPersist.SetResult();

            var bothFired = Task.WhenAll(firstFired.Task, secondFired.Task);
            var completed = await Task.WhenAny(bothFired, Task.Delay(1000));

            Assert.Equal(bothFired, completed);
            Assert.True(firstFired.Task.IsCompletedSuccessfully, "First wait callback should fire");
            Assert.True(secondFired.Task.IsCompletedSuccessfully, "Second wait callback should fire");
            Assert.Contains(entity, _handler.PersistedEntities);
        }

        [Fact]
        public async Task MarkChanged_RapidChanges_BatchedIntoPersist()
        {
            var service = CreateService();
            var entity = new TestEntity();

            service.Register(entity);

            // Rapid-fire changes
            entity.MarkChanged();
            entity.MarkChanged();
            entity.MarkChanged();

            // Wait for persistence loop to process
            await Task.Delay(200);

            // Should only persist once due to batching
            Assert.Single(_handler.PersistedEntities);
        }

        [Fact]
        public async Task MarkChanged_NoHandler_ThrowsException()
        {
            // Create service with no handlers
            var service = new PersistenceService(
                _shutdownManager.Object,
                _logger.Object,
                []
            );

            var entity = new TestEntity();

            service.Register(entity);
            service.Start();

            entity.MarkChanged();

            // Wait for persistence loop to process and log exception
            await Task.Delay(200);

            _logger.Verify(
                l => l.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Persistence failure")),
                    It.Is<InvalidOperationException>(ex => ex.Message.Contains("No persistence handler registered")),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void MarkChanged_WhileWaiting_ReturnsFalse()
        {
            var service = CreateService();
            var entity = new TestEntity();

            // Block persistence so we can test the waiting state
            var blockPersist = new TaskCompletionSource();
            _handler.BlockUntil = blockPersist;

            service.Register(entity);

            entity.MarkChanged();
            service.WaitForPersistence(entity, () => { });

            // Entity is now waiting - further changes should be rejected
            var result = entity.MarkChanged();

            Assert.False(result);

            // Cleanup
            blockPersist.SetResult();
        }

        [Fact]
        public async Task MarkChanged_AfterWaitCompletes_ReturnsTrue()
        {
            var service = CreateService();
            var entity = new TestEntity();
            var callbackFired = new TaskCompletionSource();

            service.Register(entity);

            entity.MarkChanged();
            service.WaitForPersistence(entity, callbackFired.SetResult);

            // Wait for callback to fire (IsWaiting should be cleared)
            await callbackFired.Task;

            // Entity should now accept changes again
            var result = entity.MarkChanged();

            Assert.True(result);
        }

        [Fact]
        public async Task PersistAsync_OnFailure_StillIncrementsVersion()
        {
            var service = CreateService();
            var entity = new TestEntity();
            var callbackFired = new TaskCompletionSource();

            _handler.ShouldThrow = true;

            service.Register(entity);

            entity.MarkChanged();
            service.WaitForPersistence(entity, callbackFired.SetResult);

            // Callback should still fire because version increments in finally block
            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));

            Assert.Equal(callbackFired.Task, completed);

            // Entity was not actually persisted due to exception
            Assert.DoesNotContain(entity, _handler.PersistedEntities);

            // But exception was logged
            _logger.Verify(
                l => l.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Persistence failure")),
                    It.Is<InvalidOperationException>(ex => ex.Message.Contains("Simulated persistence failure")),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private PersistenceService CreateService()
        {
            var service = new PersistenceService(
                _shutdownManager.Object,
                _logger.Object,
                [_handler]
            );
            service.Start();
            return service;
        }

        private class TestEntity : IPersistable
        {
            public event PersistableChangeCallback? OnPersistableChange;

            public void MarkChanged(
                IPersistable? association = null,
                params ReadOnlySpan<IPersistable?> additionalAssociations)
            {
                OnPersistableChange!(this, association, additionalAssociations);
            }
        }

        private class TestPersistenceHandler : IPersistenceHandler
        {
            public Type EntityType => typeof(TestEntity);

            public List<IPersistable> PersistedEntities { get; } = [];

            public TaskCompletionSource? BlockUntil { get; set; }

            public bool ShouldThrow { get; set; }

            public async Task PersistAsync(IPersistable entity)
            {
                if (BlockUntil is not null)
                {
                    await BlockUntil.Task;
                }

                if (ShouldThrow)
                {
                    throw new InvalidOperationException("Simulated persistence failure");
                }

                PersistedEntities.Add(entity);
            }
        }
    }
}
