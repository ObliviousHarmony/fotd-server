using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Extensions;
using FOMServer.Shared.Infrastructure.Factories;
using FOMServer.World.Application;
using FOMServer.World.Application.Networking;
using FOMServer.World.Core.Models;
using FOMServer.World.Infrastructure.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.World
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
            services.AddWorldServices();
            services.AddRepositories();
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
            if (string.IsNullOrWhiteSpace(serverSettings.MasterServer))
                throw new InvalidOperationException("Master server must be configured.");
            if (serverSettings.MasterServerPort <= 0)
                throw new InvalidOperationException("Master server port must be greater than 0.");
            if (string.IsNullOrWhiteSpace(dbSettings.Name))
                throw new InvalidOperationException("Database name must be configured.");
            if (string.IsNullOrWhiteSpace(dbSettings.ConnectionString))
                throw new InvalidOperationException("Database connection string must be configured.");

            services.AddSingleton(serverSettings);
            services.AddSingleton(dbSettings);
            return services;
        }

        private static ServiceCollection AddWorldServices(this ServiceCollection services)
        {
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddSingleton<MasterConnectionManager>();
            services.AddSingleton<IMasterPacketSender, MasterPacketSender>(sp =>
            {
                return new MasterPacketSender(
                    () => sp.GetRequiredService<MasterConnectionManager>()
                );
            });

            return services;
        }

        private static ServiceCollection AddRepositories(this ServiceCollection services)
        {
            return services;
        }

        private static ServiceCollection AddPacketHandlers(this ServiceCollection services)
        {
            return services;
        }
    }
}
