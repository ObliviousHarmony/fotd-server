using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Repositories;

namespace FOMServer.Shared.Infrastructure.Repositories
{
    internal class DbAccountRepository : IAccountRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbAccountRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public AccountDto? GetById(uint id)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<AccountDto?>(
                "SELECT `id`, `username`, `password`, `logged_in`, `created_at`, `updated_at` FROM `account` WHERE `id` = @id",
                new { id }
            );
        }

        public AccountDto? GetByUsername(string username)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<AccountDto?>(
                "SELECT `id`, `username`, `password`, `logged_in`, `created_at`, `updated_at` FROM `account` WHERE `username` = @username",
                new { username }
            );
        }
    }
}
