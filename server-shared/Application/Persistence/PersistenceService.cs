using System.Runtime.CompilerServices;
using System.Threading.Channels;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Application.Persistence
{
    public class PersistenceService : IPersistenceService, IServerStartable
    {
        /// <summary>
        /// The length of time that the service should wait before persisting a dirty entity.
        /// </summary>
        private const int PersistenceDelayMs = 50;

        private readonly IShutdownManager _shutdownManager;
        private readonly ILogger<PersistenceService> _logger;
        private readonly Dictionary<Type, IPersistenceHandler> _handlers;
        private readonly Channel<IPersistable> _dirtyQueue;
        private readonly Channel<WaitRequest> _waitQueue;
        private readonly ConditionalWeakTable<IPersistable, EntityState> _entityStates;

        private Task? _persistenceTask;
        private CancellationTokenSource? _cts;

        public PersistenceService(IShutdownManager shutdownManager, ILogger<PersistenceService> logger, IEnumerable<IPersistenceHandler> handlers)
        {
            _shutdownManager = shutdownManager;
            _logger = logger;
            _handlers = handlers.ToDictionary(h => h.EntityType);
            _dirtyQueue = Channel.CreateUnbounded<IPersistable>(
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            );
            _waitQueue = Channel.CreateUnbounded<WaitRequest>(
                new UnboundedChannelOptions
                {
                    SingleReader = true
                }
            );
            _entityStates = new ConditionalWeakTable<IPersistable, EntityState>();
        }

        public void Register(IPersistable entity)
        {
            entity.OnPersistableChange += Enqueue;
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
            Enqueue(entity);

            // Block future enqueues after we've queued this one
            Interlocked.Exchange(ref state.IsWaiting, 1);
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

        private bool Enqueue(
            IPersistable entity,
            IPersistable? association = null,
            IEnumerable<IPersistable>? additionalAssociations = null)
        {
            var state = _entityStates.GetOrCreateValue(entity);

            // Reject enqueue if entity is waiting for persistence
            if (Volatile.Read(in state.IsWaiting) == 1)
                return false;

            var version = Volatile.Read(in state.Version);

            // Record blocking dependencies on each association
            if (association != null)
            {
                var assocState = _entityStates.GetOrCreateValue(association);
                assocState.AddBlockingDependency(entity, version);
            }

            if (additionalAssociations != null)
            {
                foreach (var assoc in additionalAssociations)
                {
                    var assocState = _entityStates.GetOrCreateValue(assoc);
                    assocState.AddBlockingDependency(entity, version);
                }
            }

            // Use an atomic flag so that dirty entities are thread-safely queued only once.
            if (Interlocked.Exchange(ref state.IsDirty, 1) == 0)
                _dirtyQueue.Writer.TryWrite(entity);

            return true;
        }

        /// <summary>
        /// Main loop that persists entities that have been marked as dirty.
        /// </summary>
        private async Task PersistenceLoopAsync(CancellationToken ct)
        {
            var pendingWaits = new List<WaitRequest>();

            try
            {
                await foreach (var entity in _dirtyQueue.Reader.ReadAllAsync(ct))
                {
                    // Fixed delay to batch rapid updates (cancellation just skips the wait)
                    try { await Task.Delay(PersistenceDelayMs, ct); } catch (OperationCanceledException) { }

                    try
                    {
                        await PersistAsync(entity);
                    }
                    catch (Exception ex)
                    {
                        // Letting unhandled exceptions prevent further persistence
                        // could lead to data loss, so log and continue.
                        _logger.LogCritical(ex, "Persistence failure");
                    }

                    while (_waitQueue.Reader.TryRead(out var wait))
                        pendingWaits.Add(wait);

                    ProcessWaits(pendingWaits);
                }
            }
            catch (OperationCanceledException)
            {
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

                // Clear IsWaiting flag for all entities in this wait
                foreach (var dependency in pendingWaits[i].BlockingDependencies)
                {
                    if (dependency.Entity.TryGetTarget(out var entity))
                    {
                        var state = _entityStates.GetOrCreateValue(entity);
                        Interlocked.Exchange(ref state.IsWaiting, 0);
                    }
                }

                try
                {
                    pendingWaits[i].Callback();
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Wait callback failure");
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

            try
            {
                await handler.PersistAsync(entity);
            }
            finally
            {
                Interlocked.Increment(ref state.Version);
            }
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
            public int IsDirty;
            public int IsWaiting;
            public long Version;

            private readonly object _syncRoot;
            private List<BlockingDependency> _blockingDependencies;

            public EntityState()
            {
                IsDirty = 0;
                IsWaiting = 0;
                Version = 0;
                _syncRoot = new();
                _blockingDependencies = new();
            }

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
    }
}
