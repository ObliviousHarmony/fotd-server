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
    }
}
