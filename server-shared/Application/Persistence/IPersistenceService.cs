using FOMServer.Shared.Core.Interfaces;

namespace FOMServer.Shared.Application.Persistence
{
	/// <summary>
	///	Interface for a service that manages persistence of entities.
	/// </summary>
	public interface IPersistenceService
	{
		/// <summary>
		/// Registers an entity to be persisted when it changes.
		/// </summary>
		/// <param name="entity">The persistable entity to register.</param>
		void Register(IPersistable entity);
	}
}
