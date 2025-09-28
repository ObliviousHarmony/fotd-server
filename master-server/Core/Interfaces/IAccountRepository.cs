using FOMServer.Master.Core.DTOs;

namespace FOMServer.Master.Core.Interfaces
{
    public interface IAccountRepository
    {
        /// <summary>
        /// Marks all of the accounts in the database as logged out.
        /// </summary>
        void MarkAllAccountsLoggedOut();

        /// <summary>
        /// Checks to see whether or not the specified account exists.
        /// </summary>
        /// <param name="username">The username to check.</param>
        uint? AccountExists(string username);

        /// <summary>
        /// Attempts to match the login credentials to an account and returns one if successful.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="password">The password to check.</param>
        AccountDto? TryLogin(string username, string password);

        /// <summary>
        /// Logs an account out.
        /// </summary>
        /// <param name="id">The ID of the account.</param>
        bool Logout(uint id);
    }
}
