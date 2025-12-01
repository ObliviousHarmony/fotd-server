using FOMServer.Application.Core;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Extensions;
using FOMServer.Shared.Infrastructure.Database;
using FOMServer.World.Application;
using FOMServer.World.Application.Networking;
using FOMServer.World.Application.Players;
using FOMServer.World.Core;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;
using FOMServer.World.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.World
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
            services.AddWorldServices();
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

            if (Enum.IsDefined(s_serverSettings!.WorldID) == false || s_serverSettings.WorldID == WorldID.MasterServer)
                throw new InvalidOperationException("Server WorldID must be set to a valid world");
            if (string.IsNullOrWhiteSpace(s_serverSettings.ClientAddress))
                throw new InvalidOperationException("Server client address must be configured");
            if (s_serverSettings.ClientPort <= 0)
                throw new InvalidOperationException("Server client port must be greater than 0");
            if (string.IsNullOrWhiteSpace(s_serverSettings.MasterServerAddress))
                throw new InvalidOperationException("Master server address must be configured");
            if (s_serverSettings.MasterServerPort <= 0)
                throw new InvalidOperationException("Master server port must be greater than 0");
            if (string.IsNullOrWhiteSpace(s_dbSettings.Name))
                throw new InvalidOperationException("Database name must be configured");
            if (string.IsNullOrWhiteSpace(s_dbSettings.ConnectionString))
                throw new InvalidOperationException("Database connection string must be configured");

            services.AddSingleton(s_serverSettings);
            services.AddSingleton(s_dbSettings);
            return services;
        }

        private static ServiceCollection AddWorldServices(this ServiceCollection services)
        {
            services.AddSingleton<ClientPacketSender>();
            services.AddSingleton<IClientPacketSender>(sp => sp.GetRequiredService<ClientPacketSender>());
            services.AddSingleton<MasterPacketSender>();
            services.AddSingleton<IMasterPacketSender>(sp => sp.GetRequiredService<MasterPacketSender>());

            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddSingleton<IPlayerRegistry, PlayerRegistry>();
            services.AddSingleton<IWorldLoginService, WorldLoginService>();
            return services;
        }

        private static ServiceCollection AddRepositories(this ServiceCollection services)
        {
            return services;
        }
    }
}
