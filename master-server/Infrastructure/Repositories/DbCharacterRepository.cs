using Dapper;
using FOMServer.Master.Core.DTOs;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.Factories;
using MySqlConnector;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbCharacterRepository : ICharacterRepository
    {
        private IDbConnectionFactory dbConnectionFactory;

        public DbCharacterRepository(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public uint? Exists(string name)
        {
            using var connection = dbConnectionFactory.Create();
            return connection.QueryFirstOrDefault<uint?>(
                "SELECT `id` FROM `character` WHERE `name` = @name",
                new { name }
            );
        }

        public CharacterDto? Get(uint accountID)
        {
            using var connection = dbConnectionFactory.Create();
            return connection.QueryFirstOrDefault<CharacterDto?>(
                "SELECT `id`, `name`, `faction`, `sex`, `skin_color`, `face`, `hair`  FROM `character` WHERE `id` = @id",
                new { id = accountID }
            );
        }

        public CharacterDto? Create(
            uint accountID,
            Faction faction,
            string name,
            string biography,
            AvatarSex sex,
            AvatarSkin skinColor,
            byte face,
            byte hair
        )
        {
            using var connection = dbConnectionFactory.Create();

            try
            {
                var sql = @"INSERT INTO `character`
(`id`, `faction`, `name`, `biography`, `sex`, `skin_color`, `face`, `hair`) VALUE
(@id, @faction, @name, @biography, @sex, @skinColor, @face, @hair);
SELECT LAST_INSERT_ID();";

                var id = connection.ExecuteScalar<uint>(
                    sql,
                    new { id = accountID, faction, name, biography, sex, skinColor, face, hair }
                );

                return new CharacterDto
                {
                    id = id,
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
