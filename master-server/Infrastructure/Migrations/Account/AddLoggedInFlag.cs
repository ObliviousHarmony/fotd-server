using FluentMigrator;

namespace FOMServer.Master.Infrastructure.Migrations
{
    [Migration(202509262256, "Adds logged_in flag to the account table")]
    public class AddLoggedInFlag : Migration
    {
        public override void Up()
        {
            Alter.Table("account")
                .AddColumn("logged_in").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("logged_in").FromTable("account");
        }
    }
}
