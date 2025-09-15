using FOMServer.Shared.Handlers;
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
			services.AddSingleton<LogService>();
			services.AddSingleton<ILogService>(sp => sp.GetRequiredService<LogService>());
			services.AddSingleton<PacketProcessor>();

			// Packet Handlers
			services.AddSingleton<RakNetPacketHandler>();
			services.AddSingleton<IPacketHandler, FOMPacketErrorHandler>();
		}
	}
}
