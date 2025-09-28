using FOMServer.Master.Core.Models;
using FOMServer.Shared.Core.Models;

namespace FOMServer.Master.Application.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Gets a logged in account by their ID.
        /// </summary>
        /// <param name="id">The ID of the account.</param>
        Account? Get(uint id);

        /// <summary>
        /// Gets a logged in account by their network address.
        /// </summary>
        /// <param name="networkAddress">The network address of the logged in account.</param>
        Account? Get(NetworkAddress clientAddress);

        /// <summary>
        /// Attempts to log a player into the server and returns their account if successful.
        /// </summary>
        /// <param name="username">The username for the account.</param>
        /// <param name="password">The password to the account.</param>
        Account? Login(string username, string password, NetworkAddress clientAddress);

        /// <summary>
        /// Logs the given account out of the server.
        /// </summary>
        /// <param name="account">The account to log out.</param>
        bool Logout(Account account);
    }
}
