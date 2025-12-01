namespace FOMServer.Master.Core.Players
{
    public interface ILoginRepository
    {

        /// <summary>
        /// Attempts to log a player in atomically and returns their ID if successful.
        /// </summary>
        uint? TryLogin(string username, string password);

        /// <summary>
        /// Logs a player out.
        /// </summary>
        bool Logout(uint id);

        /// <summary>
        /// Marks all players in the database as logged out.
        /// </summary>
        void LogoutAllPlayers();
    }
}
