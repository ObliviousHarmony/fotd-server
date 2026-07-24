using FluentMigrator;

namespace FOMServer.Shared.Infrastructure.Migrations.Item
{
    [Migration(202607241033, "Creates the item table.")]
    public class CreateItemTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("item")
                .WithColumn("id").AsUInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("type").AsUInt16().NotNullable()
                .WithColumn("location_type").AsUInt8().NotNullable()
                .WithColumn("location_id").AsUInt32().NotNullable()
                .WithColumn("world_id").AsUInt8().Nullable()
                .WithColumn("slot").AsUInt8().NotNullable()
                .WithColumn("value").AsUInt16().NotNullable()
                .WithColumn("value_max").AsUInt16().NotNullable()
                .WithColumn("durability").AsUInt16().NotNullable()
                .WithColumn("durability_loss_factor").AsUInt8().NotNullable()
                .WithColumn("security").AsUInt8().NotNullable()
                .WithColumn("rarity").AsUInt8().NotNullable()
                .WithColumn("creator_player_id").AsUInt32().NotNullable()
                .WithColumn("stolen_from_player_id").AsUInt32().NotNullable()
                .WithColumn("timeout").AsUInt32().NotNullable()
                .WithColumn("attribute_bonus").AsUInt8().NotNullable()
                .WithColumn("recipe_variation").AsUInt8().NotNullable()
                .WithColumn("recipe_balance_1").AsUInt8().NotNullable()
                .WithColumn("recipe_balance_2").AsUInt8().NotNullable()
                .WithColumn("recipe_balance_3").AsUInt8().NotNullable()
                .WithColumn("recipe_balance_4").AsUInt8().NotNullable()

                .WithColumn("created_at").AsCreatedAtTimestamp()
                .WithColumn("updated_at").AsUpdatedAtTimestamp()
                .WithColumn("deleted_at").AsTimestamp().Nullable();

            Create.Index("idx_item_location_type_location_id_world_id").OnTable("item")
                .OnColumn("location_type").Ascending()
                .OnColumn("location_id").Ascending()
                .OnColumn("world_id").Ascending();
        }
    }
}
