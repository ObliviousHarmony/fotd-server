using FOMServer.Shared.Core.Persistence;

namespace FOMServer.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Base class for persistence handlers that write to the database.
    /// </summary>
    internal abstract class DbPersistenceHandlerBase<T> : IPersistenceHandler
        where T : IPersistable
    {
        protected readonly IDbConnectionFactory _dbConnectionFactory;

        protected DbPersistenceHandlerBase(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public Type EntityType => typeof(T);

        public async Task PersistAsync(IPersistable entity)
        {
            if (entity is not T typedEntity)
            {
                throw new InvalidOperationException(
                    $"Handler {GetType().Name} cannot persist entity of type {entity.GetType().Name}");
            }

            await PersistAsync(typedEntity);
        }

        /// <summary>
        /// Implement this in your concrete handler to persist the entity.
        /// </summary>
        protected abstract Task PersistAsync(T entity);
    }
}
