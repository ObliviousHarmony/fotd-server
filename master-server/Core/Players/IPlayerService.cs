using FOMServer.Shared.Core.Packets;

namespace FOMServer.Master.Core.Players
{
    public interface IPlayerService
    {
        /// <summary>
        /// Gets a logged in player by their ID.
        /// </summary>
        Player? Get(uint id);

        /// <summary>
        /// Gets a logged in player by their network address.
        /// </summary>
        Player? Get(NetworkAddress clientAddress);

        /// <summary>
        /// Attempts to log a player into the server and returns their player if successful.
        /// </summary>
        Player? Login(string username, string password, NetworkAddress clientAddress);

        /// <summary>
        /// Logs the given player out of the server.
        /// </summary>
        bool Logout(Player player);
    }
}
