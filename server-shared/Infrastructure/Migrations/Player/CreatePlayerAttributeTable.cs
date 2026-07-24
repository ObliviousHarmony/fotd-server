using FluentMigrator;

namespace FOMServer.Shared.Infrastructure.Migrations.Player
{
    [Migration(202601191446, "Creates the player attribute table.")]
    public class CreatePlayerAttributeTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("player_attribute")
                .WithColumn("player_id").AsUInt32().NotNullable().ForeignKey("fk_player_attribute_player", "player", "id")
                .WithColumn("type").AsUInt8().NotNullable()
                .WithColumn("value").AsUInt32().NotNullable().WithDefaultValue(0);

            Create.PrimaryKey("pk_player_attribute").OnTable("player_attribute").Columns("player_id", "type");
        }
    }
}
