using FluentMigrator;
using FOMServer.Master.Extensions;

namespace FOMServer.Master.Infrastructure.Database.Migrations.Account
{
    [Migration(202509271644, "Creates the `account` table.")]
    public class CreateAccountTable : Migration
    {
        public override void Up()
        {
            Create.Table("account")
                .WithColumn("id").AsUnsignedInt().NotNullable().PrimaryKey().Identity()
                .WithColumn("username").AsString(18).NotNullable().Unique()
                .WithColumn("password").AsString(32).NotNullable()
                .WithColumn("logged_in").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("created_at").AsCustom("TIMESTAMP").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }

        public override void Down()
        {
            Delete.Table("account");
        }
    }
}
