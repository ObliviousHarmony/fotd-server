using Dapper;
using FOMServer.Master.Core.DTOs;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Infrastructure.Factories;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbAccountRepository : IAccountRepository
    {
        private IConnectionFactory connectionFactory;

        public DbAccountRepository(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public uint? Exists(string username)
        {
            using var connection = connectionFactory.Create();
            var dto = connection.QuerySingleOrDefault<AccountDto>(
                "SELECT `id`, `username` FROM `account` WHERE `username` = @Username",
                new { Username = username }
            );
            return dto?.id;
        }

        public AccountDto? TryLogin(string username, string password)
        {
            using var connection = connectionFactory.Create();
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
            using var connection = connectionFactory.Create();
            var affected = connection.Execute(
                "UPDATE `account` SET `logged_in` = 0 WHERE `id` = @ID",
                new { ID = id }
            );
            return affected > 0;
        }

        public void MarkAllAccountsLoggedOut()
        {
            using var connection = connectionFactory.Create();
            connection.Execute("UPDATE `account` SET `logged_in` = 0");
        }
    }
}
