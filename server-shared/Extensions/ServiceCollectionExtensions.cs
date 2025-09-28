using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Application.PacketHandlers;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using FOMServer.Shared.Infrastructure.Services;
using FOMServer.Shared.Services.FOMNetwork;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
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
            services.AddSingleton<LogService>();
            services.AddSingleton<ILogService>(sp => sp.GetRequiredService<LogService>());
            services.AddSingleton<PacketProcessor>();

            services.AddSingleton<NetworkManager>();
            services.AddSingleton<IPacketSender, PacketSender>(sp =>
            {
                return new PacketSender(
                    () => sp.GetRequiredService<NetworkManager>()
                );
            });
        }

        private static void AddPacketHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IPacketHandler, ReadPacketErrorHandler>();
        }
    }
}
