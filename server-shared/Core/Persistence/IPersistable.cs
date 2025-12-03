namespace FOMServer.Shared.Core.Persistence
{
    /// <summary>
    /// Handler for persistence change events. Associations are entities that
    /// depend on the persistence of the changed entity completing first.
    /// Returns false if the change was rejected (e.g., entity is waiting).
    /// </summary>
    public delegate bool PersistableChangeCallback(
        IPersistable entity,
        IPersistable? association = null,
        IEnumerable<IPersistable>? additionalAssociations = null);

    public interface IPersistable
    {
        event PersistableChangeCallback? OnPersistableChange;
    }
}
