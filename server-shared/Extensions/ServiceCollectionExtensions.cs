using FOMServer.Shared.Handlers;
using FOMServer.Shared.Services;
using FOMServer.Shared.Services.FOMNetwork;
using FOMServer.Shared.Services.Packets;
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

			// Packet Sender
			services.AddSingleton<NetworkManager>();
			services.AddSingleton<IPacketSender, PacketSender>(sp => {
				return new PacketSender(
					() => sp.GetRequiredService<NetworkManager>()
				);
			});

			// Packet Handlers
			services.AddSingleton<IPacketHandler, ReadPacketErrorHandler>();
		}
	}
}
