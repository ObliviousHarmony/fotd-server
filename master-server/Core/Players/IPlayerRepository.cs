using FOMServer.Master.Core.DTOs;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Core.Players
{
    public interface IPlayerRepository
    {
        /// <summary>
        /// Gets the player by their ID.
        /// </summary>
        PlayerDto? GetByID(uint id);

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

        /// <summary>
        /// Loads all attributes for the given player.
        /// </summary>
        IEnumerable<PlayerAttributeDto> GetAttributes(uint playerID);

        /// <summary>
        /// Saves the given attributes for the player.
        /// </summary>
        void SaveAttributes(uint playerID, IEnumerable<PlayerAttributeDto> attributes);
    }
}
