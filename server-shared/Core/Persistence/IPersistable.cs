namespace FOMServer.Shared.Core.Persistence
{
    /// <summary>
    /// Handler for persistence change events. Associations are entities that
    /// depend on the persistence of the changed entity completing first.
    /// Returns false if the change was rejected (e.g., entity is waiting).
    /// </summary>
    public delegate void PersistableChangeCallback(
        IPersistable entity,
        params ReadOnlySpan<IPersistable?> associations
    );

    public interface IPersistable
    {
        event PersistableChangeCallback? PersistableChange;
    }
}
