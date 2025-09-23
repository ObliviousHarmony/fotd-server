using FOMServer.Shared.Core.Interfaces;

namespace FOMServer.Shared.Application.Persistence
{
    /// <summary>
    /// Contract for a handler that knows how to persist a specific entity type.
    /// </summary>
    public interface IPersistenceHandler
    {
        /// <summary>
        /// Gets the type of the entity persisted by this instance.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Persists the given entity in a thread-safe manner.
        /// </summary>
        Task PersistAsync(IPersistable entity);
    }
}
