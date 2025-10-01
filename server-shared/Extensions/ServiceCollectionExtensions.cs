using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Logging;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Infrastructure.Logging;
using FOMServer.Shared.Services.FOMNetwork;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void StartLogService(this IServiceCollection services, bool writeToConsole = true, string? logFilePath = null)
        {
            var logService = new LogService(writeToConsole, logFilePath);
            services.AddSingleton<ILogService>(logService);

            logService.Start();
        }

        public static void AddServerShared(this IServiceCollection services)
        {
            services.AddInteropServices();
            services.AddSharedServices();
            services.AddPacketHandlers();
        }

        private static void AddInteropServices(this IServiceCollection services)
        {
            services.AddSingleton<INetworkService, NetworkService>();
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IPacketService, PacketService>();
        }

        private static void AddSharedServices(this IServiceCollection services)
        {
        }

        private static void AddPacketHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IPacketHandler, ReadPacketErrorHandler>();
        }
    }
}
