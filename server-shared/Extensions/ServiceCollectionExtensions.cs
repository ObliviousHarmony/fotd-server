using System.Reflection;
using FOMServer.Application.Core;
using FOMServer.Shared.Application.Persistence;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Persistence;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Logging;
using FOMServer.Shared.Services.FOMNetwork;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void StartLogService(
            this IServiceCollection services,
            IShutdownManager shutdownManager,
            bool writeToConsole = true,
            string? logFilePath = null
        )
        {
            var logService = new LogService(shutdownManager, writeToConsole, logFilePath);
            services.AddSingleton<ILogService>(logService);

            logService.Start();
        }

        public static IServiceCollection AddServerShared(this IServiceCollection services)
        {
            services.AddInteropServices();
            services.AddSharedServices();
            services.AddPacketHandlers();
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
                services.AddSingleton(handlerInterface, type);

            return services;
        }

        private static IServiceCollection AddInteropServices(this IServiceCollection services)
        {
            services.AddSingleton<INetworkService, NetworkService>();
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IPacketService, PacketService>();
            return services;
        }

        private static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            services.AddSingleton<IShutdownManager, ShutdownManager>();

            services.AddSingleton<IPersistenceService, PersistenceService>();
            services.AddSingleton(sp => (IServerStartable)sp.GetRequiredService<IPersistenceService>());

            return services;
        }
    }
}
