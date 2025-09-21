using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentMigrator.Runner;
using FOMServer.Shared.Extensions;
using FOMServer.Master.Handlers;
using FOMServer.Shared.Services.Packets;
using MasterServer.Models;
using FOMServer.Master.Migrations;

namespace FOMServer.Master
{
	internal static class CompositionRoot
	{
		private static ServerSettings? serverSettings;
		private static DatabaseSettings? dbSettings;

		public static IServiceProvider BuildContainer()
		{
			ServiceCollection services = new ServiceCollection();

			// Run before anything else so that the cached settings in this class are available.
			services.AddConfiguration();

			services.AddServerShared();

			services.AddDatabaseMigrations();
			services.AddPacketHandlers();

			services.AddSingleton<Server>();
			return services.BuildServiceProvider();
		}

		private static ServiceCollection AddPacketHandlers(this ServiceCollection services)
		{
			services.AddSingleton<IPacketHandler, IncomingConectionHandler>();
			services.AddSingleton<IPacketHandler, LoginRequestHandler>();
			return services;
		}

		private static ServiceCollection AddConfiguration(this ServiceCollection services)
		{
			IConfigurationRoot config = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false)
				.AddJsonFile("appsettings.development.json", optional: true)
				.AddEnvironmentVariables()
				.Build();

			serverSettings = config.GetSection("Server").Get<ServerSettings>()!;
			dbSettings = config.GetSection("Database").Get<DatabaseSettings>()!;

			if (serverSettings.Port <= 0)
				throw new InvalidOperationException("Server port must be greater than 0.");
			if (string.IsNullOrWhiteSpace(dbSettings.Name))
				throw new InvalidOperationException("Database name must be configured.");
			if (string.IsNullOrWhiteSpace(dbSettings.ConnectionString))
				throw new InvalidOperationException("Database connection string must be configured.");

			services.AddSingleton(serverSettings);
			services.AddSingleton(dbSettings);
			return services;
		}

		private static ServiceCollection AddDatabaseMigrations(this ServiceCollection services)
		{
			services.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddMySql8()
                  .WithGlobalConnectionString(dbSettings!.ConnectionString)
                  .ScanIn(typeof(InitialMigration).Assembly).For.Migrations();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

			return services;
		}
	}
}
