using Microsoft.Extensions.DependencyInjection;
using FOMServer.Shared.Services;
using FOMServer.Shared.Services.FOMNetwork;

namespace FOMServer.Shared.Factories
{
	/// <summary>
	/// Factory for creating and configuring NetworkManager instances.
	/// </summary>
	public class NetworkManagerFactory
	{
		private readonly IServiceProvider serviceProvider;
		private readonly IServerService serverService;
		private readonly IClientService clientService;

		public NetworkManagerFactory(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			this.serverService = serviceProvider.GetRequiredService<IServerService>();
			this.clientService = serviceProvider.GetRequiredService<IClientService>();
		}

		/// <summary>
		/// Create a NetworkManager configured as a server.
		/// </summary>
		/// <param name="port">The port to listen on.</param>
		public NetworkManager StartServer(ushort port)
		{
			IntPtr server = serverService.Startup(port);
			if (server == IntPtr.Zero)
				throw new Exception("Failed to start server.");

			return new NetworkManager(
				server,
				serverService.Shutdown,
				serviceProvider.GetRequiredService<IPacketService>(),
				serviceProvider.GetRequiredService<PacketProcessor>()
			);
		}

		/// <summary>
		/// Create a NetworkManager configured as a client.
		/// </summary>
		/// <param name="address">The address of the server to connect to.</param>
		/// <param name="port">The port of the server to connect to.</param>
		public NetworkManager Connect(string address, ushort port)
		{
			IntPtr client = clientService.Connect(address, port);
			if (client == IntPtr.Zero)
				throw new Exception("Failed to connect to server.");

			return new NetworkManager(
				client,
				clientService.Disconnect,
				serviceProvider.GetRequiredService<IPacketService>(),
				serviceProvider.GetRequiredService<PacketProcessor>()
			);
		}
	}
}
