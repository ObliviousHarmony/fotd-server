namespace FOMServer.Shared.Core.Persistence
{
    public interface IPersistenceHandler
    {
        Type EntityType { get; }

        Task PersistAsync(IPersistable entity);
    }
}
