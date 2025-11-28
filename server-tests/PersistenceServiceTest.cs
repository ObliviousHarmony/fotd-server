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
            public event Action<IPersistable, IEnumerable<IPersistable>?>? OnChanged;

            public void MarkChanged(IEnumerable<IPersistable>? associations = null)
            {
                OnChanged?.Invoke(this, associations);
            }
        }

        private class TestPersistenceHandler : IPersistenceHandler
        {
            public Type EntityType => typeof(TestEntity);

            public List<IPersistable> PersistedEntities { get; } = new();
            public TaskCompletionSource? BlockUntil { get; set; }

            public async Task PersistAsync(IPersistable entity)
            {
                if (BlockUntil != null)
                    await BlockUntil.Task;

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
            item.MarkChanged(associations: new[] { player });

            service.WaitForPersistence(player, () => callbackFired.SetResult());

            var completed = await Task.WhenAny(callbackFired.Task, Task.Delay(1000));

            Assert.Equal(callbackFired.Task, completed);
            Assert.Contains(item, _handler.PersistedEntities);
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
            item.MarkChanged(associations: new[] { player });

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

    }
}
