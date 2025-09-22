using FOMServer.Shared.Core.Interfaces;

namespace FOMServer.Shared.Application.Persistence
{
	/// <summary>
	/// Base class providing locking around persistence operations.
	/// </summary>
	public abstract class PersistenceHandlerBase<T> : IPersistenceHandler
		where T : IPersistable
	{
		public abstract Type EntityType { get; }

		private readonly object syncRoot = new();

		public void PersistSafely(IPersistable entity)
		{
			if (entity is T tEntity)
			{
				lock (syncRoot)
				{
					Persist((dynamic)tEntity);
				}
			}
			else
			{
				throw new InvalidOperationException(
					$"{GetType().Name} cannot handle type {entity.GetType().Name}");
			}
		}

		protected abstract void Persist(T entity);
	}
}
