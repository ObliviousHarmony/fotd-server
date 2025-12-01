namespace FOMServer.Shared.Core.Persistence
{
    /// <summary>
    /// Handler for persistence change events.
    /// </summary>
    /// <param name="entity">The entity that changed.</param>
    /// <param name="association">
    /// An entity that should be considered as dependent on the persistence
    /// of the changed entity.
    /// </param>
    /// <param name="additionalAssociations">
    /// Additional entities to be considered dependent.
    /// </param>
    /// <returns>True if the change was enqueued, false if rejected (e.g., entity is waiting for persistence).</returns>
    public delegate bool PersistenceChangedHandler(
        IPersistable entity,
        IPersistable? association = null,
        IEnumerable<IPersistable>? additionalAssociations = null);

    /// <summary>
    /// Marks a domain entity as persistable.
    /// </summary>
    public interface IPersistable
    {
        /// <summary>
        /// Raised when the object's state changes and requires persistence.
        /// </summary>
        event PersistenceChangedHandler OnPersistableChange;
    }
}
