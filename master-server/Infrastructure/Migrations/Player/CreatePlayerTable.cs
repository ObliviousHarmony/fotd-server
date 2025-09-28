using FluentMigrator;

namespace FOMServer.Master.Infrastructure.Migrations
{
    [Migration(202509271652, "Creates the `player` table.")]
    public class CreatePlayerTable : Migration
    {
        public override void Up()
        {
            Create.Table("player")
                .WithColumn("id").AsCustom("INT UNSIGNED").NotNullable().PrimaryKey().ForeignKey("fk_player_account", "account", "id")
                .WithColumn("name").AsString(19).NotNullable().Unique()
                .WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }

        public override void Down()
        {
            Delete.Table("player");
        }
    }
}
