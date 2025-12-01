using FOMServer.Master.Core.DTOs;
using FOMServer.Shared.Core.Enums;

namespace FOMServer.Master.Core.Players
{
    public interface IPlayerRepository
    {
        /// <summary>
        /// Checks to see whether or not the specified player exists.
        /// </summary>
        uint? Exists(string username);

        /// <summary>
        /// Attempts to match the login credentials to a player and returns one if successful.
        /// </summary>
        PlayerDto? TryLogin(string username, string password);

        /// <summary>
        /// Logs a player out.
        /// </summary>
        bool Logout(uint id);

        /// <summary>
        /// Marks all of the player in the database as logged out.
        /// </summary>
        void LogoutAllPlayers();

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
