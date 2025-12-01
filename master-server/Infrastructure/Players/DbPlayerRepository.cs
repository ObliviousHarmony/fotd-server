using System.Text;
using Dapper;
using FOMServer.Master.Core.DTOs;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.Database;
using MySqlConnector;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbPlayerRepository : IPlayerRepository
    {
        private IDbConnectionFactory _dbConnectionFactory;

        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public PlayerDto? GetByID(uint id)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QuerySingleOrDefault<PlayerDto>(
                "SELECT `id`, `username` FROM `player` WHERE `id` = @id",
                new { id }
            );
        }

        public uint? GetIDByUsername(string username)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QuerySingleOrDefault<uint?>(
                "SELECT `id` FROM `player` WHERE `username` = @username",
                new { username }
            );
        }

        public uint? GetIDByName(string name)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QueryFirstOrDefault<uint?>(
                "SELECT `player_id` FROM `avatar` WHERE `name` = @name",
                new { name }
            );
        }

        public AvatarDto? GetAvatar(uint playerID)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.QueryFirstOrDefault<AvatarDto?>(
                "SELECT `player_id`, `name`, `faction`, `sex`, `skin_color`, `face`, `hair` FROM `avatar` WHERE `player_id` = @playerID",
                new { playerID }
            );
        }

        public AvatarDto? CreateAvatar(
            uint playerID,
            Faction faction,
            string name,
            string biography,
            AvatarSex sex,
            AvatarSkin skinColor,
            byte face,
            byte hair
        )
        {
            using var connection = _dbConnectionFactory.Create();

            try
            {
                connection.Execute(
                    @"INSERT INTO `avatar`
(`player_id`, `faction`, `name`, `biography`, `sex`, `skin_color`, `face`, `hair`) VALUE
(@playerID, @faction, @name, @biography, @sex, @skinColor, @face, @hair)",
                    new { playerID, faction, name, biography, sex, skinColor, face, hair }
                );

                return new AvatarDto
                {
                    player_id = playerID,
                    name = name,
                    faction = faction,
                    sex = sex,
                    skin_color = skinColor,
                    face = face,
                    hair = hair,
                };
            }
            catch (MySqlException)
            {
                return null;
            }
        }

        public IEnumerable<PlayerAttributeDto> GetAttributes(uint playerID)
        {
            using var connection = _dbConnectionFactory.Create();
            return connection.Query<PlayerAttributeDto>(
                "SELECT `attribute_id`, `value` FROM `player_attribute` WHERE `player_id` = @playerID",
                new { playerID }
            );
        }

        public void SaveAttributes(uint playerID, IEnumerable<PlayerAttributeDto> attributes)
        {
            var attributeList = attributes.ToList();
            if (attributeList.Count == 0)
                return;

            var sql = new StringBuilder();
            sql.Append("INSERT INTO `player_attribute` (`player_id`, `attribute_id`, `value`) VALUES ");

            var parameters = new DynamicParameters();
            parameters.Add("playerID", playerID);

            for (int i = 0; i < attributeList.Count; i++)
            {
                if (i > 0)
                    sql.Append(", ");

                sql.Append($"(@playerID, @attr{i}, @val{i})");
                parameters.Add($"attr{i}", attributeList[i].attribute_id);
                parameters.Add($"val{i}", attributeList[i].value);
            }

            sql.Append(" ON DUPLICATE KEY UPDATE `value` = VALUES(`value`)");

            using var connection = _dbConnectionFactory.Create();
            connection.Execute(sql.ToString(), parameters);
        }
    }
}
