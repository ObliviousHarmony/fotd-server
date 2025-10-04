using FluentMigrator.Runner;
using FOMServer.Application.Core;
using FOMServer.Master.Application;
using FOMServer.Master.Application.FOMPacket;
using FOMServer.Master.Application.Handlers;
using FOMServer.Master.Application.Networking;
using FOMServer.Master.Application.Players;
using FOMServer.Master.Core;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Master.Infrastructure.Factories;
using FOMServer.Master.Infrastructure.Repositories;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Extensions;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.World.Application.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.Master
{
    internal static class CompositionRoot
    {
        private static ServerSettings? s_serverSettings;
        private static DatabaseSettings? s_dbSettings;

        public static IServiceProvider BuildContainer()
        {
            ServiceCollection services = new ServiceCollection();

            var shutdownManager = new ShutdownManager();
            services.AddSingleton<IShutdownManager>(sp => shutdownManager);

            // Run before anything else so that the cached settings in this class are available.
            services.AddConfiguration();

            // Start the log service as early as possible so that everything is logged.
            services.StartLogService(shutdownManager);

            services.AddServerShared();
            services.AddMasterServices();
            services.AddFactories();
            services.AddDatabaseMigrations();
            services.AddRepositories();

            services.AddSingleton<Server>();
            return services.BuildServiceProvider();
        }

        private static ServiceCollection AddConfiguration(this ServiceCollection services)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            s_serverSettings = config.GetSection("Server").Get<ServerSettings>()!;
            s_dbSettings = config.GetSection("Database").Get<DatabaseSettings>()!;

            if (s_serverSettings.WorldPort <= 0)
                throw new InvalidOperationException("World server port must be greater than 0");
            if (s_serverSettings.ClientPort <= 0)
                throw new InvalidOperationException("Client port must be greater than 0");
            if (s_serverSettings.WorldPort == s_serverSettings.ClientPort)
                throw new InvalidOperationException("World and client ports must be different");
            if (string.IsNullOrWhiteSpace(s_dbSettings.Name))
                throw new InvalidOperationException("Database name must be configured");
            if (string.IsNullOrWhiteSpace(s_dbSettings.ConnectionString))
                throw new InvalidOperationException("Database connection string must be configured");

            services.AddSingleton(s_serverSettings);
            services.AddSingleton(s_dbSettings);
            return services;
        }

        private static ServiceCollection AddMasterServices(this ServiceCollection services)
        {
            services.AddSingleton<ClientPacketSender>();
            services.AddSingleton<IClientPacketSender>(sp => sp.GetRequiredService<ClientPacketSender>());
            services.AddSingleton<WorldPacketSender>();
            services.AddSingleton<IWorldPacketSender>(sp => sp.GetRequiredService<WorldPacketSender>());

            services.AddSingleton<IWorldServerService, WorldServerService>();
            services.AddSingleton<IPlayerService, PlayerService>();
            return services;
        }

        private static ServiceCollection AddFactories(this ServiceCollection services)
        {
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddSingleton<IWorldOverviewFactory, WorldOverviewFactory>();
            return services;
        }

        private static ServiceCollection AddDatabaseMigrations(this ServiceCollection services)
        {
            services.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                rb.AddMySql8()
                  .WithGlobalConnectionString(s_dbSettings!.ConnectionString)
                  .ScanIn(typeof(Server).Assembly).For.Migrations();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole());

            return services;
        }

        private static ServiceCollection AddRepositories(this ServiceCollection services)
        {
            services.AddSingleton<IPlayerRepository, DbPlayerRepository>();
            services.AddSingleton<ICharacterRepository, DbCharacterRepository>();
            return services;
        }
    }
}
