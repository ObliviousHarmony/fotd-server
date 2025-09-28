using Dapper;
using FOMServer.Master.Core.DTOs;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Infrastructure.Factories;

namespace FOMServer.Master.Infrastructure.Repositories
{
    public class DbCharacterRepository : ICharacterRepository
    {
        private IConnectionFactory connectionFactory;

        public DbCharacterRepository(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public uint? Exists(string name)
        {
            using var connection = connectionFactory.Create();
            return connection.QueryFirstOrDefault<uint?>(
                "SELECT `id` FROM `character` WHERE `name` = @name",
                new { name }
            );
        }

        public CharacterDto? Get(uint accountID)
        {
            using var connection = connectionFactory.Create();
            return connection.QueryFirstOrDefault<CharacterDto?>(
                "SELECT `id`, `name`, `faction`, `sex`, `skin_color`, `face`, `hair`  FROM `character` WHERE `id` = @id",
                new { id = accountID }
            );
        }

        public CharacterDto? Create(uint accountID, Faction faction, string name, AvatarSex sex, AvatarSkin skinColor, byte face, byte hair)
        {
            using var connection = connectionFactory.Create();

            var sql = @"INSERT INTO `character`
(`id`, `faction`, `name`, `sex`, `skin_color`, `face`, `hair`) VALUE
(@id, @faction, @name, @sex, @skinColor, @face, @hair);
SELECT LAST_INSERT_ID();";

            var id = connection.ExecuteScalar<uint>(
                sql,
                new { id = accountID, faction, name, sex, skinColor, face, hair }
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
    }
}
