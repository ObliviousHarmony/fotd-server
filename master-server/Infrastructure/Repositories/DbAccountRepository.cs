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

        public uint? AccountExists(string username)
        {
            using (var connection = connectionFactory.Create())
            {
                var dto = connection.QuerySingleOrDefault<AccountDto>(
                    "SELECT `id`, `username` FROM `account` WHERE `username` = @Username",
                    new { Username = username }
                );
                return dto?.id;
            }
        }

        public AccountDto? TryLogin(string username, string password)
        {
            using (var connection = connectionFactory.Create())
            {
                return connection.QuerySingleOrDefault<AccountDto>(
                    "SELECT `id`, `username` FROM `account` WHERE `username` = @Username",
                    new { Username = username }
                );
            }
        }
    }
}
