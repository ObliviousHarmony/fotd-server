using Dapper;
using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Players;
using FOMServer.Shared.Infrastructure.Database;

namespace FOMServer.Shared.Infrastructure.Players
{
    public abstract class DbPlayerRepositoryBase : IPlayerRepositoryBase
    {
        protected readonly IDbConnectionFactory _dbConnectionFactory;

        public DbPlayerRepositoryBase(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public PlayerDTO? GetByID(uint id)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QuerySingleOrDefault<PlayerDTO>(
                "SELECT `id`, `username` FROM `player` WHERE `id` = @id",
                new { id }
            );
        }

        public AvatarDTO? GetAvatar(uint playerID)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QueryFirstOrDefault<AvatarDTO?>(
                "SELECT `name`, `faction`, `sex`, `skin_color`, `face`, `hair` FROM `avatar` WHERE `player_id` = @playerID",
                new { playerID }
            );
        }
    }
}
