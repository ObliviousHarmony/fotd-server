using FOMServer.Master.Core.DTOs;

namespace FOMServer.Master.Core.Interfaces
{
    public interface IAccountRepository
    {
        /// <summary>
        /// Checks to see whether or not the specified account exists.
        /// </summary>
        uint? Exists(string username);

        /// <summary>
        /// Attempts to match the login credentials to an account and returns one if successful.
        /// </summary>
        AccountDto? TryLogin(string username, string password);

        /// <summary>
        /// Logs an account out.
        /// </summary>
        bool Logout(uint id);

        /// <summary>
        /// Marks all of the accounts in the database as logged out.
        /// </summary>
        void MarkAllAccountsLoggedOut();
    }
}
