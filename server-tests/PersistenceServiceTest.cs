using FOMServer.Shared.Application.Persistence;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Persistence;
using Moq;

namespace FOMServer.Tests
{
    public class PersistenceServiceTest : IDisposable
    {
        private class TestEntity : IPersistable
        {
            public event PersistenceChangedHandler? OnPersistableChange;

            public bool MarkChanged(
                IPersistable? association = null,
                IEnumerable<IPersistable>? additionalAssociations = null)
            {
                return OnPersistableChange?.Invoke(this, association, additionalAssociations) ?? true;
            }
        }

        private class TestPersistenceHandler : IPersistenceHandler
        {
            public Type EntityType => typeof(TestEntity);

            public List<IPersistable> PersistedEntities { get; } = new();
            public TaskCompletionSource? BlockUntil { get; set; }
            public bool ShouldThrow { get; set; }

            public async Task PersistAsync(IPersistable entity)
            {
                if (BlockUntil != null)
                    await BlockUntil.Task;

                if (ShouldThrow)
                    throw new InvalidOperationException("Simulated persistence failure");

                PersistedEntities.Add(entity);
            }
        }

        private readonly Mock<IShutdownManager> _shutdownManager;
        private readonly Mock<ILogService> _logService;
        private readonly TestPersistenceHandler _handler;
        private readonly CancellationTokenSource _cts;

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }

        public PersistenceServiceTest()
        {
            _cts = new CancellationTokenSource();
            _shutdownManager = new Mock<IShutdownManager>();
            _shutdownManager.Setup(s => s.Token).Returns(_cts.Token);
            _shutdownManager.Setup(s => s.TrackTask(It.IsAny<Task>()));

            _logService = new Mock<ILogService>();
            _handler = new TestPersistenceHandler();
        }

        private PersistenceService CreateService()
        {
            var service = new PersistenceService(
                _shutdownManager.Object,
                _logService.Object,
                new[] { _handler }
            );
            service.Start();
            return service;
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
            service.WaitForPersistence(entity, () => callbackFired.SetResult());

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

            service.WaitForPersistence(player, () => callbackFired.SetResult());

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
            item1.MarkChanged(null, new[] { player });

            // Item2 changes with both primary and additional associations
            item2.MarkChanged(player, new[] { item1 });

            service.WaitForPersistence(player, () => callbackFired.SetResult());

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

            service.WaitForPersistence(player, () => callbackFired.SetResult());

            // Wait a bit - callback should NOT have fired yet
            await Task.Delay(200);
            Assert.False(callbackFired.Task.IsCompleted, "Callback should not fire until item is persisted");

            // Release the block
            blockItem.SetResult();

            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));
            Assert.Equal(callbackFired.Task, completed);
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
                _logService.Object,
                Enumerable.Empty<IPersistenceHandler>()
            );

            var entity = new TestEntity();

            service.Register(entity);
            service.Start();

            entity.MarkChanged();

            // Wait for persistence loop to process and log exception
            await Task.Delay(200);

            _logService.Verify(
                l => l.WriteException(It.Is<InvalidOperationException>(
                    ex => ex.Message.Contains("No persistence handler registered"))),
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
            service.WaitForPersistence(entity, () => callbackFired.SetResult());

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
            service.WaitForPersistence(entity, () => callbackFired.SetResult());

            // Callback should still fire because version increments in finally block
            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));

            Assert.Equal(callbackFired.Task, completed);

            // Entity was not actually persisted due to exception
            Assert.DoesNotContain(entity, _handler.PersistedEntities);

            // But exception was logged
            _logService.Verify(
                l => l.WriteException(It.Is<InvalidOperationException>(
                    ex => ex.Message.Contains("Simulated persistence failure"))),
                Times.Once);
        }

    }
}
