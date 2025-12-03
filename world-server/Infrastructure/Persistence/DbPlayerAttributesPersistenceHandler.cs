using System.Data;
using System.Text;
using Dapper;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Shared.Infrastructure.Persistence;
using FOMServer.World.Core.Players;
using MySqlConnector;

namespace FOMServer.World.Infrastructure.Persistence
{
    public class DbPlayerAttributesPersistenceHandler : DbPersistenceHandlerBase<PlayerAttributes>
    {
        private static readonly string s_sql;

        [ThreadStatic]
        private static AttributeParameters? s_parameters;

        static DbPlayerAttributesPersistenceHandler()
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO `player_attribute` (`player_id`, `attribute_id`, `value`) VALUES ");

            for (int i = 0; i < PlayerAttributes.AttributeCount; i++)
            {
                if (i > 0)
                    sql.Append(", ");
                sql.Append("(?, ?, ?)");
            }

            sql.Append(" ON DUPLICATE KEY UPDATE `value` = VALUES(`value`)");
            s_sql = sql.ToString();
        }

        public DbPlayerAttributesPersistenceHandler(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        protected override async Task PersistAsync(PlayerAttributes entity)
        {
            var parameters = s_parameters ??= new AttributeParameters();
            parameters.Entity = entity;

            using var connection = _dbConnectionFactory.Create();
            await connection.ExecuteAsync(s_sql, parameters);
        }

        private class AttributeParameters : SqlMapper.IDynamicParameters
        {
            private readonly MySqlParameter[] _params;

            public AttributeParameters()
            {
                _params = new MySqlParameter[PlayerAttributes.AttributeCount * 3];
                for (int i = 0; i < _params.Length; i++)
                    _params[i] = new MySqlParameter();
            }

            public PlayerAttributes? Entity { get; set; }

            public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
            {
                var entity = Entity!;
                var parameters = command.Parameters;

                for (int i = 0; i < PlayerAttributes.AttributeCount; i++)
                {
                    int baseIndex = i * 3;
                    _params[baseIndex].Value = entity.PlayerID;
                    _params[baseIndex + 1].Value = i;
                    _params[baseIndex + 2].Value = entity.Get((PlayerAttribute)i);

                    parameters.Add(_params[baseIndex]);
                    parameters.Add(_params[baseIndex + 1]);
                    parameters.Add(_params[baseIndex + 2]);
                }
            }
        }
    }
}
