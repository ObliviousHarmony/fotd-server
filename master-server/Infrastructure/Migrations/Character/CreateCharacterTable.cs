using FluentMigrator;
using FOMServer.Master.Extensions;

namespace FOMServer.Master.Infrastructure.Migrations
{
    [Migration(202509280939, "Creates the `character` table.")]
    public class CreateCharacterTable : Migration
    {
        public override void Up()
        {
            Create.Table("character")
                .WithColumn("id").AsUnsignedInt().NotNullable().PrimaryKey().ForeignKey("fk_character_account", "account", "id")
                .WithColumn("name").AsString(19).NotNullable().Unique()
                .WithColumn("faction").AsUnsignedByte().NotNullable()
                .WithColumn("sex").AsUnsignedByte().NotNullable()
                .WithColumn("skin_color").AsUnsignedByte().NotNullable()
                .WithColumn("face").AsUnsignedByte().NotNullable()
                .WithColumn("hair").AsUnsignedByte().NotNullable()
                .WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }

        public override void Down()
        {
            Delete.Table("character");
        }
    }
}
