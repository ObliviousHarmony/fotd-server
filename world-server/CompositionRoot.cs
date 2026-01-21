using FOMServer.Application.Core;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Extensions;
using FOMServer.Shared.Infrastructure;
using FOMServer.World.Application;
using FOMServer.World.Application.Networking;
using FOMServer.World.Core;
using FOMServer.World.Core.Networking;
using FOMServer.World.Infrastructure;

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

            // Configure logging as early as possible so that everything is logged.
            services.ConfigureLogging(shutdownManager);

            services.AddServerShared();
            services.AddWorldServices();
            services.AddRepositories();
            services.AddPersistenceHandlers();

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

            if (s_serverSettings!.WorldIDs.Length == 0)
                throw new InvalidOperationException("At least one WorldID must be configured");
            foreach (var worldID in s_serverSettings.WorldIDs)
            {
                if (!Enum.IsDefined(worldID) || worldID == WorldID.MasterServer || worldID == WorldID.NUM_WORLDS)
                    throw new InvalidOperationException($"Invalid WorldID: {worldID}");
            }
            if (s_serverSettings.WorldIDs.Distinct().Count() != s_serverSettings.WorldIDs.Length)
                throw new InvalidOperationException("Duplicate WorldIDs are not allowed");
            if (string.IsNullOrWhiteSpace(s_serverSettings.PublicHost))
                throw new InvalidOperationException("Public host must be configured");
            if (string.IsNullOrWhiteSpace(s_serverSettings.MasterServerHost))
                throw new InvalidOperationException("Master server host must be configured");
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
            return services;
        }

        private static ServiceCollection AddRepositories(this ServiceCollection services)
        {
            return services;
        }

        private static ServiceCollection AddPersistenceHandlers(this ServiceCollection services)
        {
            return services;
        }
    }
}
