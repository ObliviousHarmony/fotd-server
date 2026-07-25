using System.Numerics;
using FOMServer.Shared.Core.Dtos;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using MySqlConnector;

namespace FOMServer.Shared.Infrastructure.Repositories
{
    internal class DbItemRepository : IItemRepository
    {
        private const string SelectColumns = """
            `id`,
            `type`,
            `location_type`,
            `location_id`,
            `slot`,
            `value`,
            `value_max`,
            `durability`,
            `durability_loss_factor`,
            `security`,
            `rarity`,
            `creator_player_id`,
            `stolen_from_player_id`,
            `timeout`,
            `attribute_bonus`,
            `recipe_variation`,
            `recipe_balance_1`,
            `recipe_balance_2`,
            `recipe_balance_3`,
            `recipe_balance_4`
            """;

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbItemRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public uint? Create(ItemDto item, WorldId? worldId = null)
        {
            using var connection = _dbConnectionFactory.Create();

            connection.Execute(
                """
                INSERT INTO `item` (
                    `type`, `location_type`, `location_id`, `world_id`, `slot`,
                    `value`, `value_max`, `durability`, `durability_loss_factor`,
                    `security`, `rarity`, `creator_player_id`, `stolen_from_player_id`,
                    `timeout`, `attribute_bonus`, `recipe_variation`,
                    `recipe_balance_1`, `recipe_balance_2`, `recipe_balance_3`, `recipe_balance_4`
                ) VALUES (
                    @type, @location_type, @location_id, @world_id, @slot,
                    @value, @value_max, @durability, @durability_loss_factor,
                    @security, @rarity, @creator_player_id, @stolen_from_player_id,
                    @timeout, @attribute_bonus, @recipe_variation,
                    @recipe_balance_1, @recipe_balance_2, @recipe_balance_3, @recipe_balance_4
                )
                """,
                new
                {
                    item.type,
                    item.location_type,
                    item.location_id,
                    world_id = worldId,
                    item.slot,
                    item.value,
                    item.value_max,
                    item.durability,
                    item.durability_loss_factor,
                    item.security,
                    item.rarity,
                    item.creator_player_id,
                    item.stolen_from_player_id,
                    item.timeout,
                    item.attribute_bonus,
                    item.recipe_variation,
                    item.recipe_balance_1,
                    item.recipe_balance_2,
                    item.recipe_balance_3,
                    item.recipe_balance_4,
                }
            );

            return connection.ExecuteScalar<uint>("SELECT LAST_INSERT_ID()");
        }

        public ItemDto? GetById(uint id)
        {
            using var connection = _dbConnectionFactory.Create();

            return connection.QuerySingleOrDefault<ItemDto?>(
                $"SELECT {SelectColumns} FROM `item` WHERE `id` = @id AND `deleted_at` IS NULL",
                new { id }
            );
        }

        public IReadOnlyDictionary<uint, ItemDto> GetByLocation(
            ItemLocationType location,
            uint locationId,
            WorldId? worldId = null
        )
        {
            using var connection = _dbConnectionFactory.Create();

            return connection
                .Query<ItemDto>(
                    $"""
                    SELECT {SelectColumns}
                    FROM `item`
                    WHERE `location_type` = @location
                      AND `location_id` = @locationId
                      AND `world_id` <=> @worldId
                      AND `deleted_at` IS NULL
                    """,
                    new
                    {
                        location,
                        locationId,
                        worldId,
                    }
                )
                .ToDictionary(item => item.id);
        }
    }
}
