using Dapper;
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

        public uint? TryLogin(string username, string password)
        {
            using var connection = _dbConnectionFactory.Create();

            // Atomically set logged_in = 1 only if currently 0 to prevent race conditions
            var affected = connection.Execute(
                "UPDATE `player` SET `logged_in` = 1 WHERE `username` = @username AND `logged_in` = 0",
                new { username }
            );
            if (affected == 0)
                return null;

            // Player is now exclusively logged in, safe to fetch their ID
            var id = connection.QuerySingleOrDefault<uint?>(
                "SELECT `id` FROM `player` WHERE `username` = @username",
                new { username }
            );

            return id;
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
