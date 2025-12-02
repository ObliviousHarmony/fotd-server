using FOMServer.Shared.Core.DTOs;

namespace FOMServer.Shared.Core.Players
{
    public interface IPlayerRepositoryBase
    {
        /// <summary>
        /// Gets the player by their ID.
        /// </summary>
        PlayerDTO? GetByID(uint id);

        /// <summary>
        /// Loads the avatar for the given player ID.
        /// </summary>
        AvatarDTO? GetAvatar(uint playerID);
    }
}
