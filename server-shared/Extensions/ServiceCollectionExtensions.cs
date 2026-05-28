using System.Reflection;
using FOMServer.Shared.Application;
using FOMServer.Shared.Application.Persistence;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Logging;
using FOMServer.Shared.Infrastructure.Repositories;
using FOMServer.Shared.Services.FOMNetwork;

namespace FOMServer.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureLogging(
            this IServiceCollection services,
            IShutdownManager shutdownManager,
            bool writeToConsole = true,
            string? logFilePath = null
        )
        {
            var provider = new BackgroundLoggerProvider(shutdownManager, writeToConsole, logFilePath);
            _ = services.AddLogging(lb => lb.ClearProviders().AddProvider(provider));

            provider.Start();
        }

        public static IServiceCollection AddServerShared(this IServiceCollection services)
        {
            _ = services.AddSingleton(TimeProvider.System);

            _ = services.AddInteropServices();
            _ = services.AddSharedServices();
            _ = services.AddSharedRepositories();
            _ = services.AddPacketHandlers();
            return services;
        }

        private static IServiceCollection AddInteropServices(this IServiceCollection services)
        {
            _ = services.AddSingleton<INetworkService, NetworkService>();
            _ = services.AddSingleton<IServerService, ServerService>();
            _ = services.AddSingleton<IClientService, ClientService>();
            _ = services.AddSingleton<IPacketService, PacketService>();
            return services;
        }

        private static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            _ = services.AddSingleton<IPersistenceService, PersistenceService>();
            _ = services.AddSingleton(sp => (IServerStartable)sp.GetRequiredService<IPersistenceService>());

            return services;
        }

        private static IServiceCollection AddSharedRepositories(this IServiceCollection services)
        {
            _ = services.AddSingleton<IAccountRepository, DbAccountRepository>();
            _ = services.AddSingleton<IPlayerRepository, DbPlayerRepository>();
            return services;
        }

        private static IServiceCollection AddPacketHandlers(this IServiceCollection services)
        {
            // Dynamically register all packet handlers found in
            // the ServerShared and Application assemblies.
            var handlerInterface = typeof(IPacketHandler);
            var assemblies = new[] {
                Assembly.GetEntryAssembly(),
                handlerInterface.Assembly,
            };

            var handlerTypes = assemblies
                .SelectMany(a => a!.GetTypes())
                .Where(t => handlerInterface.IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            foreach (var type in handlerTypes)
            {
                _ = services.AddSingleton(handlerInterface, type);
            }

            return services;
        }
    }
}
