using FOMServer.Master.Core.DTOs;

namespace FOMServer.Master.Core.Interfaces
{
    public interface IAccountRepository
    {
        /// <summary>
        /// Checks to see whether or not the specified account exists.
        /// </summary>
        /// <param name="username">The username to check.</param>
        public uint? AccountExists(string username);

        /// <summary>
        /// Attempts to match the login credentials to an account and returns one if successful.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="password">The password to check.</param>
        public AccountDto? TryLogin(string username, string password);
    }
}
