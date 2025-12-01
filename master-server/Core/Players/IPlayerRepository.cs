using FOMServer.Master.Core.DTOs;
using FOMServer.Shared.Core.Enums;
using PlayerRepositoryBase = FOMServer.Shared.Core.Players.IPlayerRepository;

namespace FOMServer.Master.Core.Players
{
    public interface IPlayerRepository : PlayerRepositoryBase
    {
        /// <summary>
        /// Gets the player ID for the given username.
        /// </summary>
        uint? GetIDByUsername(string username);

        /// <summary>
        /// Gets the player ID for the given avatar name.
        /// </summary>
        uint? GetIDByName(string name);

        /// <summary>
        /// Loads the avatar for the given player ID.
        /// </summary>
        AvatarDTO? GetAvatar(uint playerID);

        /// <summary>
        /// Creates a new avatar for the given player.
        /// </summary>
        AvatarDTO? CreateAvatar(
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
