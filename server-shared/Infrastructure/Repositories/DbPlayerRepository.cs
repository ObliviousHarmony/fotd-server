using System.Data;
using Dapper;
using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Repositories;
using MySqlConnector;

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
                 "SELECT `id`, `name`, `sex`, `race`, `face`, `hair`, `created_at`, `updated_at` FROM `player` WHERE `id` = @id",
                 new { id }
             );
        }

        public PlayerDTO? GetByName(string name)
        {
            return _connection.QuerySingleOrDefault<PlayerDTO?>(
                 "SELECT `id`, `name`, `sex`, `race`, `face`, `hair`, `created_at`, `updated_at` FROM `player` WHERE `name` = @name",
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

        public PlayerDTO? Create(uint id, string name, string biography, AvatarSex sex, AvatarRace race, ushort face, ushort hair)
        {
            try
            {
                _connection.Execute(
                    @"INSERT INTO `player` (`id`, `name`, `biography`, `sex`, `race`, `face`, `hair`)
                      VALUES (@id, @name, @biography, @sex, @race, @face, @hair)",
                    new { id, name, biography, sex = (byte)sex, race = (byte)race, face, hair }
                );
            }
            catch (MySqlException)
            {
                return null;
            }

            return GetByID(id)!;
        }
    }
}
