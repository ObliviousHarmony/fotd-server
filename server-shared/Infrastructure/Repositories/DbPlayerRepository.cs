using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using MySqlConnector;

namespace FOMServer.Shared.Infrastructure.Repositories
{
    internal class DbPlayerRepository : IPlayerRepository
    {
        private const string SelectColumns = """
            `id`,
            `name`,
            `sex`,
            `race`,
            `face`,
            `hair`
            """;

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public uint? Create(PlayerDto player, string biography)
        {
            try
            {
                using var connection = _dbConnectionFactory.Create();

                connection.Execute(
                    """
                    INSERT INTO `player` (
                        `id`, `name`, `biography`, `sex`, `race`, `face`, `hair`
                    ) VALUES (
                        @id, @name, @biography, @sex, @race, @face, @hair
                    )
                    """,
                    new
                    {
                        player.id,
                        player.name,
                        biography,
                        player.sex,
                        player.race,
                        player.face,
                        player.hair,
                    }
                );

                return player.id;
            }
            catch (MySqlException e) when (e.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                return null;
            }
        }

        public PlayerDto? GetById(uint id)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<PlayerDto?>(
                $"SELECT {SelectColumns} FROM `player` WHERE `id` = @id",
                new { id }
            );
        }

        public PlayerDto? GetByName(string name)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<PlayerDto?>(
                $"SELECT {SelectColumns} FROM `player` WHERE `name` = @name",
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
