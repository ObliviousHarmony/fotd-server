using FOMServer.Shared.Core.Player;

namespace FOMServer.Master.Core.Player
{
    public interface IPlayerRepository : IPlayerRepositoryBase
    {
        uint? GetIDByUsername(string username);
    }
}
