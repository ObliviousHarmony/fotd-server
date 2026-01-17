using FOMServer.Shared.Core.Players;

namespace FOMServer.Master.Core.Players
{
    public interface IPlayerRepository : IPlayerRepositoryBase
    {
        uint? GetIDByUsername(string username);
    }
}
