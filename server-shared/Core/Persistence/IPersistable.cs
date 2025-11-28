namespace FOMServer.Shared.Core.Persistence
{
    /// <summary>
    /// Handler for persistence change events.
    /// </summary>
    /// <param name="entity">The entity that changed.</param>
    /// <param name="associations">
    /// Entities that should be considered as dependent on the persistence
    /// of the changed entity.
    /// </param>
    /// <returns>True if the change was enqueued, false if rejected (e.g., entity is waiting for persistence).</returns>
    public delegate bool PersistenceChangedHandler(IPersistable entity, IEnumerable<IPersistable>? associations);

    /// <summary>
    /// Marks a domain entity as persistable.
    /// </summary>
    public interface IPersistable
    {
        /// <summary>
        /// Raised when the object's state changes and requires persistence.
        /// </summary>
        event PersistenceChangedHandler OnChanged;
    }
}
