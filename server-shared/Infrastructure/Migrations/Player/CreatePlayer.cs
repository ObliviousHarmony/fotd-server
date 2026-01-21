using FluentMigrator;

namespace FOMServer.Shared.Infrastructure.Migrations.Player
{
    [Migration(202601191445, "Creates the player tables.")]
    public class CreatePlayer : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("player")
                .WithColumn("id").AsUnsignedInt().NotNullable().PrimaryKey().ForeignKey("account", "id")
                .WithColumn("name").AsString(19).NotNullable().Unique()
                .WithColumn("biography").AsText().NotNullable()
                .WithColumn("created_at").AsCreatedAtTimestamp()
                .WithColumn("updated_at").AsUpdatedAtTimestamp();
        }
    }
}
