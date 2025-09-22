using FluentMigrator;

namespace FOMServer.Master.Infrastructure.Migrations
{
	[Migration(202509201809, "Initial migration creating account and player tables")]
	public class InitialMigration : Migration
	{
		public override void Up()
		{
			Create.Table("account")
				.WithColumn("id").AsCustom("INT UNSIGNED").NotNullable().PrimaryKey().Identity()
				.WithColumn("username").AsString(18).NotNullable()
				.WithColumn("password").AsString(255).NotNullable()
				.WithColumn("created_at").AsCustom("TIMESTAMP").NotNullable()
					.WithDefault(SystemMethods.CurrentUTCDateTime)
				.WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

			Create.UniqueConstraint("uq_account_username")
				.OnTable("account").Column("username");

			Create.Table("player")
				.WithColumn("id").AsCustom("INT UNSIGNED").NotNullable()
				.WithColumn("name").AsString(32).NotNullable()
				.WithColumn("updated_at").AsCustom("TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

			Create.PrimaryKey("pk_player_id").OnTable("player").Column("id");

			Create.ForeignKey("fk_player_account")
				.FromTable("player").ForeignColumn("id")
				.ToTable("account").PrimaryColumn("id")
				.OnDeleteOrUpdate(System.Data.Rule.Cascade);
		}

		public override void Down()
		{
			Delete.Table("player");
			Delete.Table("account");
		}
	}
}
