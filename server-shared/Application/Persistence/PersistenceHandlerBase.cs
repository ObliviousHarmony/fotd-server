using FOMServer.Shared.Core.Interfaces;

namespace FOMServer.Shared.Application.Persistence
{
    /// <summary>
    /// Base class providing locking around persistence operations.
    /// </summary>
    public abstract class PersistenceHandlerBase<T> : IPersistenceHandler
        where T : IPersistable
    {
        public Type EntityType => typeof(T);

        public async Task PersistAsync(IPersistable entity)
        {
            if (entity is not T typedEntity)
                throw new InvalidOperationException(
                    $"Handler {GetType().Name} cannot persist entity of type {entity.GetType().Name}");

            await PersistAsync(typedEntity);
        }

        /// <summary>
        /// Implement this in your concrete handler to persist the entity.
        /// </summary>
        protected abstract Task PersistAsync(T entity);
    }
}
