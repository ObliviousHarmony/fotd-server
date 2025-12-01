using FOMServer.Master.Core.DTOs;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Core.Players
{
    public interface IPlayerRepository
    {
        /// <summary>
        /// Gets the player ID for the given avatar name.
        /// </summary>
        uint? GetIDByName(string name);

        /// <summary>
        /// Loads the avatar for the given player ID.
        /// </summary>
        AvatarDto? GetAvatar(uint playerID);

        /// <summary>
        /// Creates a new avatar for the given player.
        /// </summary>
        AvatarDto? CreateAvatar(
            uint playerID,
            Faction faction,
            string name,
            string biography,
            AvatarSex sex,
            AvatarSkin skinColor,
            byte face,
            byte hair
        );
    }
}
