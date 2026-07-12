using FluentMigrator;

namespace FOMServer.Shared.Infrastructure.Migrations.Account
{
    [Migration(202601191443, "Creates the account tables.")]
    public class CreateAccount : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create
                .Table("account")
                .WithColumn("id")
                .AsUInt32()
                .NotNullable()
                .PrimaryKey()
                .Identity()
                .WithColumn("username")
                .AsString(18)
                .NotNullable()
                .Unique()
                .WithColumn("password")
                .AsString(64)
                .NotNullable()
                .WithColumn("logged_in")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false)
                .WithColumn("created_at")
                .AsCreatedAtTimestamp()
                .WithColumn("updated_at")
                .AsUpdatedAtTimestamp();
        }
    }
}
