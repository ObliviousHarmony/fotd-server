using FluentMigrator;
using FOMServer.Master.Extensions;

namespace FOMServer.Master.Infrastructure.Database.Migrations.Player
{
    [Migration(202512021200, "Creates the player tables.")]
    public class CreatePlayer : Migration
    {
        public override void Up()
        {
            Create.Table("player")
                .WithColumn("id").AsUnsignedInt().NotNullable().PrimaryKey().Identity()
                .WithColumn("username").AsString(18).NotNullable().Unique()
                .WithColumn("password").AsString(32).NotNullable()
                .WithColumn("logged_in").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("created_at").AsCustom("TIMESTAMP").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            Create.Table("player_avatar")
                .WithColumn("player_id").AsUnsignedInt().NotNullable().PrimaryKey().ForeignKey("fk_player_avatar_player", "player", "id")
                .WithColumn("name").AsString(19).NotNullable().Unique()
                .WithColumn("biography").AsString(510).NotNullable()
                .WithColumn("faction").AsUnsignedByte().NotNullable()
                .WithColumn("sex").AsUnsignedByte().NotNullable()
                .WithColumn("skin_color").AsUnsignedByte().NotNullable()
                .WithColumn("face").AsUnsignedByte().NotNullable()
                .WithColumn("hair").AsUnsignedByte().NotNullable()
                .WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

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
            Delete.Table("player_avatar");
            Delete.Table("player");
        }
    }
}
