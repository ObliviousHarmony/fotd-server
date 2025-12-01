using FluentMigrator;
using FOMServer.Master.Extensions;

namespace FOMServer.Master.Infrastructure.Database.Migrations.PlayerAttribute
{
    [Migration(202511302014, "Creates the `player_attribute` table.")]
    public class CreatePlayerAttributeTable : Migration
    {
        public override void Up()
        {
            Create.Table("player_attribute")
                .WithColumn("player_id").AsUnsignedInt().NotNullable().ForeignKey("fk_player_attribute_player", "player", "id")
                .WithColumn("attribute_id").AsByte().NotNullable()
                .WithColumn("value").AsInt32().NotNullable();

            Create.PrimaryKey("pk_player_attribute")
                .OnTable("player_attribute")
                .Columns("player_id", "attribute_id");
        }

        public override void Down()
        {
            Delete.Table("player_attribute");
        }
    }
}
