namespace FOMServer.Shared.Core.Persistence
{
    public interface IPersistenceService
    {
        void Register(IPersistable entity);

        void Unregister(IPersistable entity);

        /// <summary>
        /// Queues a callback to be invoked once the entity and all
        /// of its dependencies have been persisted. The callback is
        /// invoked on the persistence thread.
        /// </summary>
        void WaitForPersistence(IPersistable entity, Action callback);
    }
}
