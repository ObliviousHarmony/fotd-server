using System.Data;
using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Repositories;

namespace FOMServer.Shared.Infrastructure.Repositories
{
    public class DbAccountRepository : IAccountRepository
    {
        private readonly IDbConnection _connection;

        public DbAccountRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.Create();
        }

        public AccountDTO? GetByID(uint id)
        {
            return _connection.QuerySingleOrDefault<AccountDTO?>(
                "SELECT `id`, `username`, `password`, `logged_in`, `created_at`, `updated_at` FROM `account` WHERE `id` = @id",
                new { id }
            );
        }

        public AccountDTO? GetByUsername(string username)
        {
            return _connection.QuerySingleOrDefault<AccountDTO?>(
                "SELECT `id`, `username`, `password`, `logged_in`, `created_at`, `updated_at` FROM `account` WHERE `username` = @username",
                new { username }
            );
        }
    }
}
