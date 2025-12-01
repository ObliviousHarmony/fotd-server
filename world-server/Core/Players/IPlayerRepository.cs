using FOMServer.World.Core.DTOs;
using PlayerRepositoryBase = FOMServer.Shared.Core.Players.IPlayerRepository;

namespace FOMServer.World.Core.Players
{
    public interface IPlayerRepository : PlayerRepositoryBase
    {
        /// <summary>
        /// Loads all attributes for the given player.
        /// </summary>
        IEnumerable<PlayerAttributeDTO> GetAttributes(uint playerID);
    }
}
