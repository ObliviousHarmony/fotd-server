namespace FOMServer.Shared.Core.Persistence
{
    /// <summary>
    /// Interface for a service that manages persistence of entities.
    /// </summary>
    public interface IPersistenceService
    {
        /// <summary>
        /// Registers an entity to be persisted when it changes.
        /// </summary>
        void Register(IPersistable entity);

        /// <summary>
        /// Queues a callback to be invoked once the entity and all
        /// of its dependencies have been persisted.
        /// </summary>
        /// <param name="entity">The entity to wait for the persistence of.</param>
        /// <param name="callback">
        /// Callback invoked on the persistence thread once persistence is completed.
        /// </param>
        void WaitForPersistence(IPersistable entity, Action callback);
    }
}
