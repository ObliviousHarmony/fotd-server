namespace FOMServer.Shared.Core.Interfaces
{
    /// <summary>
    /// Marks a domain entity as persistable.
    /// </summary>
    public interface IPersistable
    {
        /// <summary>
        /// Raised when the object’s state changes and requires persistence.
        /// </summary>
        event Action<IPersistable> OnChanged;
    }
}
