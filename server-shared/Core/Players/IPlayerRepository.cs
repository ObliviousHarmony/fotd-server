using FOMServer.Shared.Core.DTOs;

namespace FOMServer.Shared.Core.Players
{
    public interface IPlayerRepository
    {
        /// <summary>
        /// Gets the player by their ID.
        /// </summary>
        PlayerDTO? GetByID(uint id);
    }
}
