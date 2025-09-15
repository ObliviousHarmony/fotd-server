using FOMServer.Shared.Factories;
using FOMServer.Shared.Services;
using FOMServer.Shared.Services.FOMNetwork;
using Microsoft.Extensions.DependencyInjection;

namespace FOMServer.Shared.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddServerShared(this IServiceCollection services)
		{
			// FOMNetwork API Services
			services.AddSingleton<INetworkService, NetworkService>();
			services.AddSingleton<IServerService, ServerService>();
			services.AddSingleton<IClientService, ClientService>();
			services.AddSingleton<IPacketService, PacketService>();

			// Shared Services
			services.AddSingleton<ILogService, LogService>();
			services.AddSingleton<PacketProcessor>();
			services.AddSingleton<NetworkManagerFactory>();
		}
	}
}
