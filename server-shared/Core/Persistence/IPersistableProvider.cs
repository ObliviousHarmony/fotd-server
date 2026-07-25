namespace FOMServer.Shared.Core.Persistence
{
    public interface IPersistableProvider
    {
        void CollectPersistables(ICollection<IPersistable> destination);
    }
}
