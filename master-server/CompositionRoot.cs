using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FluentMigrator.Runner;
using FOMServer.Master.Application;
using FOMServer.Master.Infrastructure.Factories;
using FOMServer.Master.Application.PacketHandlers;
using FOMServer.Master.Core.Models;
using FOMServer.Shared.Infrastructure.Factories;
using FOMServer.Shared.Extensions;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Master.Infrastructure.Migrations;
using FOMServer.Master.Core.Interfaces;
using FOMServer.Master.Infrastructure.Repositories;
using FOMServer.Master.Application.Services;

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
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            services.AddDatabaseMigrations();
            services.AddRepositories();
            services.AddMasterServices();
            services.AddPacketHandlers();

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

        private static ServiceCollection AddRepositories(this ServiceCollection services)
        {
            services.AddSingleton<IAccountRepository, DbAccountRepository>();
            return services;
        }

        private static ServiceCollection AddMasterServices(this ServiceCollection services)
        {
            services.AddSingleton<IAccountService, AccountService>();
            return services;
        }

        private static ServiceCollection AddPacketHandlers(this ServiceCollection services)
        {
            services.AddSingleton<IPacketHandler, DisconnectionHandler>();
            services.AddSingleton<IPacketHandler, LoginRequestHandler>();
            services.AddSingleton<IPacketHandler, LoginHandler>();
            services.AddSingleton<IPacketHandler, CheckNameHandler>();
            return services;
        }
    }
}
