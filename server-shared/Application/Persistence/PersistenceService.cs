using System.Runtime.CompilerServices;
using System.Threading.Channels;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Application.Persistence
{
    /// <summary>
    /// Coordinates persistence requests across entities.
    /// </summary>
    public class PersistenceService : IPersistenceService
    {
        private const int PersistenceDelayMs = 50;

        private readonly IShutdownManager _shutdownManager;
        private readonly ILogService _logService;

        /// <summary>
        /// A dependency that blocks an entity from completing persistence.
        /// </summary>
        private class BlockingDependency
        {
            public required WeakReference<IPersistable> Entity;
            public required long Version;
        }

        /// <summary>
        /// Tracks persistence state for each entity.
        /// </summary>
        private class EntityState
        {
            public int IsDirty = 0;
            public long Version = 0;

            private readonly object _syncRoot = new();
            private List<BlockingDependency> _blockingDependencies = new();

            public void AddBlockingDependency(IPersistable entity, long version)
            {
                lock (_syncRoot)
                {
                    _blockingDependencies.Add(new BlockingDependency
                    {
                        Entity = new WeakReference<IPersistable>(entity),
                        Version = version
                    });
                }
            }

            public List<BlockingDependency> TakeBlockingDependencies()
            {
                lock (_syncRoot)
                {
                    var result = _blockingDependencies;
                    _blockingDependencies = new List<BlockingDependency>();
                    return result;
                }
            }
        }

        /// <summary>
        /// Represents a pending wait request.
        /// </summary>
        private class WaitRequest
        {
            public List<BlockingDependency> BlockingDependencies = new();
            public required Action Callback;
        }

        private readonly Dictionary<Type, IPersistenceHandler> _handlers;
        private readonly Channel<IPersistable> _dirtyQueue;
        private readonly Channel<WaitRequest> _waitQueue;
        private readonly ConditionalWeakTable<IPersistable, EntityState> _entityStates;

        private Task? _persistenceTask;
        private CancellationTokenSource? _cts;

        public PersistenceService(IShutdownManager shutdownManager, ILogService logService, IEnumerable<IPersistenceHandler> handlers)
        {
            _shutdownManager = shutdownManager;
            _logService = logService;
            _handlers = handlers.ToDictionary(h => h.EntityType);
            _dirtyQueue = Channel.CreateUnbounded<IPersistable>();
            _waitQueue = Channel.CreateUnbounded<WaitRequest>();
            _entityStates = new ConditionalWeakTable<IPersistable, EntityState>();
        }

        public void Register(IPersistable entity)
        {
            entity.OnChanged += Enqueue;
        }

        private void Enqueue(IPersistable entity, IEnumerable<IPersistable>? associations)
        {
            var state = _entityStates.GetOrCreateValue(entity);
            var version = Volatile.Read(in state.Version);

            // Record blocking dependencies on each association
            if (associations != null)
            {
                foreach (var assoc in associations)
                {
                    var assocState = _entityStates.GetOrCreateValue(assoc);
                    assocState.AddBlockingDependency(entity, version);
                }
            }

            // Use an atomic flag so that dirty entities are thread-safely queued only once.
            if (Interlocked.Exchange(ref state.IsDirty, 1) == 0)
                _dirtyQueue.Writer.TryWrite(entity);
        }

        public void WaitForPersistence(IPersistable entity, Action callback)
        {
            var state = _entityStates.GetOrCreateValue(entity);
            var blockingDependencies = state.TakeBlockingDependencies();

            // Also wait for the entity itself to persist
            blockingDependencies.Add(new BlockingDependency
            {
                Entity = new WeakReference<IPersistable>(entity),
                Version = Volatile.Read(in state.Version)
            });

            _waitQueue.Writer.TryWrite(new WaitRequest
            {
                BlockingDependencies = blockingDependencies,
                Callback = callback
            });

            // Ensure the entity goes through the persistence loop so waits get processed
            Enqueue(entity, null);
        }

        /// <summary>
        /// Starts the background persistence task.
        /// </summary>
        public void Start()
        {
            if (_persistenceTask != null)
                return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownManager.Token);

            // Use the thread pool for this task as it does a ton of blocking IO.
            _persistenceTask = Task.Run(() => PersistenceLoopAsync(_cts.Token), _cts.Token);

            // Make sure that the shutdown manager waits for this task to complete.
            _shutdownManager.TrackTask(_persistenceTask);
        }

        /// <summary>
        /// Main loop that persists entities that have been marked as dirty.
        /// </summary>
        private async Task PersistenceLoopAsync(CancellationToken ct)
        {
            var pendingWaits = new List<WaitRequest>();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var entity = await _dirtyQueue.Reader.ReadAsync(ct);

                    // Fixed delay to batch rapid updates
                    await Task.Delay(PersistenceDelayMs, ct);

                    await PersistAsync(entity);

                    while (_waitQueue.Reader.TryRead(out var wait))
                        pendingWaits.Add(wait);

                    ProcessWaits(pendingWaits);
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
                    _logService.WriteException(ex);
                    continue;
                }
            }

            // Drain and persist remaining entities before shutdown
            while (_dirtyQueue.Reader.TryRead(out var entity))
                await PersistAsync(entity);
        }

        /// <summary>
        /// Drains the wait queue and fires callbacks for any completed waits.
        /// </summary>
        private void ProcessWaits(List<WaitRequest> pendingWaits)
        {
            for (int i = pendingWaits.Count - 1; i >= 0; i--)
            {
                if (!IsWaitComplete(pendingWaits[i]))
                    continue;

                try
                {
                    pendingWaits[i].Callback();
                }
                catch (Exception ex)
                {
                    _logService.WriteException(ex);
                }
                pendingWaits.RemoveAt(i);
            }
        }

        /// <summary>
        /// Persists an entity if it is dirty.
        /// </summary>
        private async Task PersistAsync(IPersistable entity)
        {
            var state = _entityStates.GetOrCreateValue(entity);
            if (Interlocked.Exchange(ref state.IsDirty, 0) == 0)
                return;

            if (!_handlers.TryGetValue(entity.GetType(), out var handler))
                throw new InvalidOperationException($"No persistence handler registered for {entity.GetType().Name}");

            await handler.PersistAsync(entity);
            Interlocked.Increment(ref state.Version);
        }

        /// <summary>
        /// Checks if all blocking dependencies for a wait request have been persisted.
        /// </summary>
        private bool IsWaitComplete(WaitRequest request)
        {
            foreach (var dependency in request.BlockingDependencies)
            {
                // If the entity was garbage collected, treat as satisfied
                if (!dependency.Entity.TryGetTarget(out var entity))
                    continue;

                var state = _entityStates.GetOrCreateValue(entity);
                if (Volatile.Read(in state.Version) <= dependency.Version)
                    return false;
            }
            return true;
        }
    }
}
