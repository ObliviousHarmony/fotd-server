using FOMServer.Master.Core.Models;
using FOMServer.Shared.Core.Models;

namespace FOMServer.Master.Application.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Gets a logged in account by their ID.
        /// </summary>
        Account? Get(uint id);

        /// <summary>
        /// Gets a logged in account by their network address.
        /// </summary>
        Account? Get(NetworkAddress clientAddress);

        /// <summary>
        /// Attempts to log a player into the server and returns their account if successful.
        /// </summary>
        Account? Login(string username, string password, NetworkAddress clientAddress);

        /// <summary>
        /// Logs the given account out of the server.
        /// </summary>
        bool Logout(Account account);
    }
}
