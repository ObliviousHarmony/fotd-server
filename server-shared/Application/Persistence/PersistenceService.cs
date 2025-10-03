using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Persistence;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace FOMServer.Shared.Application.Persistence
{
    /// <summary>
    /// Coordinates persistence requests across entities.
    /// </summary>
    public class PersistenceService : IPersistenceService
    {
        private readonly IShutdownManager shutdownManager;
        private readonly ILogService logService;

        private class DirtyFlag { public int IsDirty = 0; }

        private readonly Dictionary<Type, IPersistenceHandler> handlers;
        private readonly Channel<IPersistable> dirtyQueue;

        private readonly ConditionalWeakTable<IPersistable, DirtyFlag> dirtyFlags;
        private readonly ConditionalWeakTable<IPersistable, SemaphoreSlim> entityLocks;

        private Task? persistenceTask;
        private CancellationTokenSource? cts;

        public PersistenceService(IShutdownManager shutdownManager, ILogService logService, IEnumerable<IPersistenceHandler> handlers)
        {
            this.shutdownManager = shutdownManager;
            this.logService = logService;
            this.handlers = handlers.ToDictionary(h => h.EntityType);
            dirtyQueue = Channel.CreateUnbounded<IPersistable>();
            dirtyFlags = new ConditionalWeakTable<IPersistable, DirtyFlag>();
            entityLocks = new ConditionalWeakTable<IPersistable, SemaphoreSlim>();
        }

        public void Register(IPersistable entity)
        {
            entity.OnChanged += Enqueue;
        }

        private void Enqueue(IPersistable entity)
        {
            // Use an atomic flag so that dirty entities are thread-safely queued only once.
            var flag = dirtyFlags.GetOrCreateValue(entity);
            if (Interlocked.Exchange(ref flag.IsDirty, 1) == 0)
                dirtyQueue.Writer.TryWrite(entity);
        }

        /// <summary>
        /// Starts the background persistence task.
        /// </summary>
        public void Start()
        {
            if (persistenceTask != null)
                return;

            cts = CancellationTokenSource.CreateLinkedTokenSource(shutdownManager.Token);

            // Use the thread pool for this task as it does a ton of blocking IO.
            persistenceTask = Task.Run(() => PersistenceLoopAsync(cts.Token), cts.Token);

            // Make sure that the shutdown manager waits for this task to complete.
            shutdownManager.TrackTask(persistenceTask);
        }

        /// <summary>
        /// Main loop that persists entities that have been marked as dirty.
        /// </summary>
        private async Task PersistenceLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var entity = await dirtyQueue.Reader.ReadAsync(ct);
                    await Handle(entity);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ChannelClosedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Letting unhandled exceptions prevent further persistence
                    // could lead to data loss, so log and continue.
                    logService.WriteException(ex);
                    continue;
                }
            }

            // Drain the queue and persist any remaining changed entities before shutting down.
            while (dirtyQueue.Reader.TryRead(out var entity))
                await Handle(entity);
        }

        /// <summary>
        /// Handles persisting a single entity.
        /// </summary>
        private async Task Handle(IPersistable entity)
        {
            if (!handlers.TryGetValue(entity.GetType(), out var handler))
                return;

            // Clear the dirty flag so that the entity can be re-queued if it changes again.
            if (!dirtyFlags.TryGetValue(entity, out var entityFlag))
                return;
            if (Interlocked.Exchange(ref entityFlag.IsDirty, 0) == 0)
                return;

            // Only allow the entity to be persisted by one thread at a time.
            var semaphore = entityLocks.GetValue(entity, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                await handler.PersistAsync(entity);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
