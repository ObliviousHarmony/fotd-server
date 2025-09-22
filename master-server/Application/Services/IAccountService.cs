using FOMServer.Master.Core.Models;

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
		/// Attempts to log a player into the server and returns their account if successful.
		/// </summary>
		/// <param name="username">The username for the account.</param>
		/// <param name="password">The password to the account.</param>
		Account? Login(string username, string password);

		/// <summary>
		/// Logs the given account out of the server.
		/// </summary>
		/// <param name="account">The account to log out.</param>
		bool Logout(Account account);
	}
}
