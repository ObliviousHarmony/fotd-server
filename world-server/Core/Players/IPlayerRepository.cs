using FOMServer.Shared.Core.Players;
using FOMServer.World.Core.DTOs;

namespace FOMServer.World.Core.Players
{
    public interface IPlayerRepository : IPlayerRepositoryBase
    {
        IEnumerable<PlayerAttributeDTO> GetAttributes(uint playerID);
    }
}
