namespace FOMServer.Shared.Core.Persistence
{
    /// <summary>
    /// Marks a domain entity as persistable.
    /// </summary>
    public interface IPersistable
    {
        /// <summary>
        /// Raised when the object's state changes and requires persistence.
        /// </summary>
        /// <param name="entity">The entity that changed.</param>
        /// <param name="associations">
        /// Entities that should be considered as dependent on the persistence
        /// of the changed entity.
        /// </param>
        event Action<IPersistable, IEnumerable<IPersistable>?> OnChanged;
    }
}
