using System.Text;
using Dapper;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Shared.Infrastructure.Persistence;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Infrastructure.Persistence
{
    public class DbPlayerAttributesPersistenceHandler : DbPersistenceHandlerBase<PlayerAttributes>
    {
        public DbPlayerAttributesPersistenceHandler(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        protected override async Task PersistAsync(PlayerAttributes entity)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO `player_attribute` (`player_id`, `attribute_id`, `value`) VALUES ");

            var parameters = new DynamicParameters();
            parameters.Add("playerID", entity.PlayerID);

            int count = (int)PlayerAttribute.NUM_ATTRIBUTES;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    sql.Append(", ");

                sql.Append($"(@playerID, @attr{i}, @val{i})");
                parameters.Add($"attr{i}", (byte)i);
                parameters.Add($"val{i}", (int)entity.Get((PlayerAttribute)i));
            }

            sql.Append(" ON DUPLICATE KEY UPDATE `value` = VALUES(`value`)");

            using var connection = _dbConnectionFactory.Create();
            await connection.ExecuteAsync(sql.ToString(), parameters);
        }
    }
}
