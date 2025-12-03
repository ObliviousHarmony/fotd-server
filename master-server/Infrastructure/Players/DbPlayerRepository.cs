using Dapper;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.DTOs;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.Shared.Infrastructure.Players;
using MySqlConnector;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbPlayerRepository : DbPlayerRepositoryBase, IPlayerRepository
    {
        public DbPlayerRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
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
                "SELECT `player_id` FROM `player_avatar` WHERE `name` = @name",
                new { name }
            );
        }

        public AvatarDTO? CreateAvatar(
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
                    @"INSERT INTO `player_avatar`
(`player_id`, `faction`, `name`, `biography`, `sex`, `skin_color`, `face`, `hair`) VALUE
(@playerID, @faction, @name, @biography, @sex, @skinColor, @face, @hair)",
                    new { playerID, faction, name, biography, sex, skinColor, face, hair }
                );

                return new AvatarDTO
                {
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
