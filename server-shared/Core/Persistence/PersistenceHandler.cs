namespace FOMServer.Shared.Core.Persistence
{
    public abstract class PersistenceHandler<T> : IPersistenceHandler
    where T : class, IPersistable
    {
        public Type EntityType => typeof(T);

        public Task PersistAsync(IPersistable entity)
        {
            return PersistAsync((T)entity);
        }

        protected abstract Task PersistAsync(T entity);
    }
}
