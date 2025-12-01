using FOMServer.World.Core.DTOs;

namespace FOMServer.World.Core.Players
{
    public interface IPlayerRepository
    {
        /// <summary>
        /// Loads all attributes for the given player.
        /// </summary>
        IEnumerable<PlayerAttributeDto> GetAttributes(uint playerID);
    }
}
