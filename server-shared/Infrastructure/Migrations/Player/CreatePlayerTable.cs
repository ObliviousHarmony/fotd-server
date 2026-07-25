using FluentMigrator;

namespace FOMServer.Shared.Infrastructure.Migrations.Player
{
    [Migration(202601191445, "Creates the player tables.")]
    public class CreatePlayerTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("player")
                .WithColumn("id").AsUInt32().NotNullable().PrimaryKey().ForeignKey("fk_player_account", "account", "id")
                .WithColumn("name").AsString(19).NotNullable().Unique()
                .WithColumn("biography").AsText().NotNullable()
                .WithColumn("sex").AsUInt8().NotNullable()
                .WithColumn("race").AsUInt8().NotNullable()
                .WithColumn("face").AsUInt16().NotNullable()
                .WithColumn("hair").AsUInt16().NotNullable()
                .WithColumn("quickslot_1").AsUInt16().NotNullable().WithDefaultValue(0)
                .WithColumn("quickslot_2").AsUInt16().NotNullable().WithDefaultValue(0)
                .WithColumn("quickslot_3").AsUInt16().NotNullable().WithDefaultValue(0)
                .WithColumn("quickslot_4").AsUInt16().NotNullable().WithDefaultValue(0)
                .WithColumn("created_at").AsCreatedAtTimestamp()
                .WithColumn("updated_at").AsUpdatedAtTimestamp();
        }
    }
}
