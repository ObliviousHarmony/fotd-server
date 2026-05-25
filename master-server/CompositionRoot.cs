using FluentMigrator.Runner;
using FOMServer.Master.Application;
using FOMServer.Master.Application.Networking;
using FOMServer.Master.Application.Players;
using FOMServer.Master.Core;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Master.Infrastructure;
using FOMServer.Shared.Application;
using FOMServer.Shared.Core;
using FOMServer.Shared.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace FOMServer.Master
{
    internal static class CompositionRoot
    {
        private static DatabaseSettings? s_dbSettings;

        public static ServiceProvider BuildContainer()
        {
            var services = new ServiceCollection();

            var shutdownManager = new ShutdownManager();
            _ = services.AddSingleton<IShutdownManager>(sp => shutdownManager);

            // Run before anything else so that the cached settings in this class are available.
            _ = services.AddConfiguration();

            // Configure logging as early as possible so that everything is logged.
            services.ConfigureLogging(shutdownManager);

            _ = services.AddServerShared();
            _ = services.AddMasterServices();
            _ = services.AddFactories();
            _ = services.AddDatabaseMigrations();
            _ = services.AddRepositories();

            _ = services.AddSingleton<Server>();
            return services.BuildServiceProvider();
        }

        private static ServiceCollection AddConfiguration(this ServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            s_dbSettings = config.GetSection("Database").Get<DatabaseSettings>()!;

            if (string.IsNullOrWhiteSpace(s_dbSettings.Name))
            {
                throw new InvalidOperationException("Database name must be configured");
            }

            if (string.IsNullOrWhiteSpace(s_dbSettings.ConnectionString))
            {
                throw new InvalidOperationException("Database connection string must be configured");
            }

            _ = services.AddSingleton(s_dbSettings);
            return services;
        }

        private static ServiceCollection AddMasterServices(this ServiceCollection services)
        {
            _ = services.AddSingleton<ClientPacketSender>();
            _ = services.AddSingleton<IClientPacketSender>(sp => sp.GetRequiredService<ClientPacketSender>());
            _ = services.AddSingleton<WorldPacketSender>();
            _ = services.AddSingleton<IWorldPacketSender>(sp => sp.GetRequiredService<WorldPacketSender>());

            _ = services.AddSingleton<IWorldServerRegistry, WorldServerRegistry>();

            _ = services.AddSingleton<IClientRegistry, ClientRegistry>();
            _ = services.AddSingleton<IPlayerRegistry, PlayerRegistry>();
            return services;
        }

        private static ServiceCollection AddFactories(this ServiceCollection services)
        {
            _ = services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            return services;
        }

        private static ServiceCollection AddDatabaseMigrations(this ServiceCollection services)
        {
            _ = services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb.AddMySql8()
                  .WithGlobalConnectionString(s_dbSettings!.ConnectionString)
                  .ScanIn(typeof(ServiceCollectionExtensions).Assembly)
                  .For.Migrations());

            return services;
        }

        private static ServiceCollection AddRepositories(this ServiceCollection services)
        {
            return services;
        }
    }
}
