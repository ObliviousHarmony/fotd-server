using System.Data;
using Dapper;
using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Repositories;

namespace FOMServer.Shared.Infrastructure.Repositories
{
    public class DbPlayerRepository : IPlayerRepository
    {
        private readonly IDbConnection _connection;

        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connection = dbConnectionFactory.Create();
        }

        public PlayerDTO? GetByID(uint id)
        {
            return _connection.QuerySingleOrDefault<PlayerDTO?>(
                 "SELECT `id`, `name`, `created_at`, `updated_at` FROM `player` WHERE `id` = @id",
                 new { id }
             );
        }

        public PlayerDTO? GetByName(string name)
        {
            return _connection.QuerySingleOrDefault<PlayerDTO?>(
                 "SELECT `id`, `name`, `created_at`, `updated_at` FROM `player` WHERE `name` = @name",
                 new { name }
             );
        }

        public string? GetBiography(uint id)
        {
            return _connection.QuerySingleOrDefault<string?>(
                 "SELECT `biography` FROM `player` WHERE `name` = @id",
                 new { id }
             );
        }
    }
}
