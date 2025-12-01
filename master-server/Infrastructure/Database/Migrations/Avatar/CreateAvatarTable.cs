using FluentMigrator;
using FOMServer.Master.Extensions;

namespace FOMServer.Master.Infrastructure.Database.Migrations.Avatar
{
    [Migration(202509280939, "Creates the `avatar` table.")]
    public class CreateAvatarTable : Migration
    {
        public override void Up()
        {
            Create.Table("avatar")
                .WithColumn("player_id").AsUnsignedInt().NotNullable().PrimaryKey().ForeignKey("fk_avatar_player", "player", "id")
                .WithColumn("name").AsString(19).NotNullable().Unique()
                .WithColumn("biography").AsString(510).NotNullable()
                .WithColumn("faction").AsUnsignedByte().NotNullable()
                .WithColumn("sex").AsUnsignedByte().NotNullable()
                .WithColumn("skin_color").AsUnsignedByte().NotNullable()
                .WithColumn("face").AsUnsignedByte().NotNullable()
                .WithColumn("hair").AsUnsignedByte().NotNullable()
                .WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }

        public override void Down()
        {
            Delete.Table("avatar");
        }
    }
}
