using FOMServer.Shared.Core.Players;
using FOMServer.World.Core.DTOs;

namespace FOMServer.World.Core.Players
{
    public interface IPlayerRepository : IPlayerRepositoryBase
    {
        /// <summary>
        /// Loads all attributes for the given player.
        /// </summary>
        IEnumerable<PlayerAttributeDTO> GetAttributes(uint playerID);
    }
}
