using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Repositories;
using MySqlConnector;

namespace FOMServer.Shared.Infrastructure.Repositories
{
    internal class DbPlayerRepository : IPlayerRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public PlayerDto? Create(
            uint id,
            string name,
            string biography,
            AvatarConstants.Sex sex,
            AvatarConstants.Race race,
            ushort face,
            ushort hair
        )
        {
            try
            {
                using var connection = _dbConnectionFactory.Create();

                connection.Execute(
                    @"INSERT INTO `player` (`id`, `name`, `biography`, `sex`, `race`, `face`, `hair`)
                      VALUES (@id, @name, @biography, @sex, @race, @face, @hair)",
                    new
                    {
                        id,
                        name,
                        biography,
                        sex = (byte)sex,
                        race = (byte)race,
                        face,
                        hair,
                    }
                );
            }
            catch (MySqlException)
            {
                return null;
            }

            return GetById(id)!;
        }

        public PlayerDto? GetById(uint id)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<PlayerDto?>(
                "SELECT `id`, `name`, `sex`, `race`, `face`, `hair`, `created_at`, `updated_at` FROM `player` WHERE `id` = @id",
                new { id }
            );
        }

        public PlayerDto? GetByName(string name)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<PlayerDto?>(
                "SELECT `id`, `name`, `sex`, `race`, `face`, `hair`, `created_at`, `updated_at` FROM `player` WHERE `name` = @name",
                new { name }
            );
        }

        public string? GetBiography(uint id)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<string?>(
                "SELECT `biography` FROM `player` WHERE `name` = @id",
                new { id }
            );
        }
    }
}
