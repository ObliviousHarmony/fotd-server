using Dapper;
using FOMServer.Master.Core.DTOs;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Infrastructure.Database;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbLoginRepository : ILoginRepository
    {
        private IDbConnectionFactory _dbConnectionFactory;

        public DbLoginRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public uint? GetIDByUsername(string username)
        {
            using var connection = _dbConnectionFactory.Create();
            var dto = connection.QuerySingleOrDefault<PlayerDto>(
                "SELECT `id`, `username` FROM `player` WHERE `username` = @username",
                new { username }
            );
            return dto?.id;
        }

        public PlayerDto? TryLogin(string username, string password)
        {
            using var connection = _dbConnectionFactory.Create();
            var player = connection.QuerySingleOrDefault<PlayerDto>(
                "SELECT `id`, `username` FROM `player` WHERE `username` = @username",
                new { username }
            );
            if (player == null)
                return null;

            connection.Execute("UPDATE `player` SET `logged_in` = 1 WHERE `id` = @id", new { id = player.id });

            return player;
        }

        public bool Logout(uint id)
        {
            using var connection = _dbConnectionFactory.Create();
            var affected = connection.Execute(
                "UPDATE `player` SET `logged_in` = 0 WHERE `id` = @id",
                new { id }
            );
            return affected > 0;
        }

        public void LogoutAllPlayers()
        {
            using var connection = _dbConnectionFactory.Create();
            connection.Execute("UPDATE `player` SET `logged_in` = 0");
        }
    }
}
