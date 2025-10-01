using Dapper;
using FOMServer.Master.Core.Repositories;
using FOMServer.Master.Core.Repositories.DTOs;
using FOMServer.Shared.Infrastructure.Database;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbAccountRepository : IAccountRepository
    {
        private IDbConnectionFactory dbConnectionFactory;

        public DbAccountRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public uint? Exists(string username)
        {
            using var connection = dbConnectionFactory.Create();
            var dto = connection.QuerySingleOrDefault<AccountDto>(
                "SELECT `id`, `username` FROM `account` WHERE `username` = @Username",
                new { Username = username }
            );
            return dto?.id;
        }

        public AccountDto? TryLogin(string username, string password)
        {
            using var connection = dbConnectionFactory.Create();
            var account = connection.QuerySingleOrDefault<AccountDto>(
                "SELECT `id`, `username` FROM `account` WHERE `username` = @Username",
                new { Username = username }
            );
            if (account == null)
                return null;

            connection.Execute("UPDATE `account` SET `logged_in` = 1 WHERE `id` = @ID", new { ID = account.id });

            return account;
        }

        public bool Logout(uint id)
        {
            using var connection = dbConnectionFactory.Create();
            var affected = connection.Execute(
                "UPDATE `account` SET `logged_in` = 0 WHERE `id` = @ID",
                new { ID = id }
            );
            return affected > 0;
        }

        public void MarkAllAccountsLoggedOut()
        {
            using var connection = dbConnectionFactory.Create();
            connection.Execute("UPDATE `account` SET `logged_in` = 0");
        }
    }
}
