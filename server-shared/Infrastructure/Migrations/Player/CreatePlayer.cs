using FluentMigrator;

namespace FOMServer.Shared.Infrastructure.Migrations.Player
{
    [Migration(202601191445, "Creates the player tables.")]
    internal class CreatePlayer : ForwardOnlyMigration
    {
        public override void Up()
        {
            _ = Create.Table("player")
                .WithColumn("id").AsUInt32().NotNullable().PrimaryKey().ForeignKey("account", "id")
                .WithColumn("name").AsString(19).NotNullable().Unique()
                .WithColumn("biography").AsText().NotNullable()
                .WithColumn("sex").AsUInt8().NotNullable()
                .WithColumn("race").AsUInt8().NotNullable()
                .WithColumn("face").AsUInt16().NotNullable()
                .WithColumn("hair").AsUInt16().NotNullable()
                .WithColumn("created_at").AsCreatedAtTimestamp()
                .WithColumn("updated_at").AsUpdatedAtTimestamp();
        }
    }
}
