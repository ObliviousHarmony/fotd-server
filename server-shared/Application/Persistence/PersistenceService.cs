using FOMServer.Shared.Core.Interfaces;
using FOMServer.Shared.Core.Models;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace FOMServer.Shared.Application.Persistence
{
	/// <summary>
	/// Coordinates persistence requests across entities.
	/// </summary>
	public class PersistenceService : IPersistenceService, IDisposable
	{
		private class DirtyFlag { public int IsDirty = 0; }

		private readonly Dictionary<Type, IPersistenceHandler> handlers;
		private readonly Channel<IPersistable> queue;
		private readonly ConditionalWeakTable<IPersistable, DirtyFlag> dirtyFlags;

		private Task? persistenceTask;
		private CancellationTokenSource? cts;

		public PersistenceService(IEnumerable<IPersistenceHandler> handlers)
		{
			this.handlers = handlers.ToDictionary(h => h.EntityType);
			this.queue = Channel.CreateUnbounded<IPersistable>();
			this.dirtyFlags = new ConditionalWeakTable<IPersistable, DirtyFlag>();
		}

		public void Register(IPersistable entity)
		{
			entity.OnChanged += Enqueue;
		}

		private void Enqueue(IPersistable entity)
		{
			// Use an atomic operation to check and set the dirty flag.
			// This is cleaned when the entity starts being persisted.
			// It thus prevents multiple enqueues of the same entity.
			DirtyFlag flag = dirtyFlags.GetOrCreateValue(entity);
			if (Interlocked.Exchange(ref flag.IsDirty, 1) == 0)
				queue.Writer.TryWrite(entity);
		}

		/// <summary>
		/// Starts the background persistence task.
		/// </summary>
		/// <param name="ctParent">The parent's cancellation token.</param>
		public void Start(CancellationToken ctParent)
		{
			if (persistenceTask != null)
				return;

			cts = CancellationTokenSource.CreateLinkedTokenSource(ctParent);

			persistenceTask = Task.Factory.StartNew(
				async () => await PersistenceLoopAsync(cts.Token),
				cts.Token,
				TaskCreationOptions.LongRunning,
				TaskScheduler.Default
			).Unwrap();
		}

		/// <summary>
		/// Stops the persistence service gracefully.
		/// </summary>
		public async Task StopAsync()
		{
			if (persistenceTask == null)
				return;

			cts?.Cancel();
			queue.Writer.Complete();

			try
			{
				await persistenceTask;
			}
			catch (OperationCanceledException)
			{
			}

			persistenceTask = null;
			cts?.Dispose();
			cts = null;
		}

		/// <summary>
		/// Main loop that persists entities that have been marked as dirty.
		/// </summary>
		private async Task PersistenceLoopAsync(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{

			}
		}

		/// <summary>
		/// Dispose of resources and stop the service if needed.
		/// </summary>
		public void Dispose()
		{
			if (persistenceTask != null)
				StopAsync().GetAwaiter().GetResult();

			GC.SuppressFinalize(this);
		}
	}
}
