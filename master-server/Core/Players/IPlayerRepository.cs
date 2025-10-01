using FOMServer.Master.Core.DTOs;

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
    }
}
