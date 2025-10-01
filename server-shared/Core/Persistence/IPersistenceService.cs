namespace FOMServer.Shared.Core.Persistence
{
    /// <summary>
    ///	Interface for a service that manages persistence of entities.
    /// </summary>
    public interface IPersistenceService
    {
        /// <summary>
        /// Registers an entity to be persisted when it changes.
        /// </summary>
        void Register(IPersistable entity);
    }
}
