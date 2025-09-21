using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models;

namespace FOMServer.Shared.Application.Services
{
	public class PacketSender : IPacketSender
	{
		private readonly Func<NetworkManager> networkManagerFactory;
		private NetworkManager? networkManager;

		public PacketSender(Func<NetworkManager> networkManagerFactory)
		{
			this.networkManagerFactory = networkManagerFactory;
		}

		/// <inheritdoc />
		public void Send(PacketIdentifier id, FOMData data, NetworkAddress destination, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0)
		{
			if (networkManager == null)
				networkManager = networkManagerFactory();

			networkManager.Send(id, data, destination, priority, reliability, orderingChannel);
		}

		/// <inheritdoc />
		public void Broadcast(PacketIdentifier id, FOMData data, NetworkAddress excludedAddress, PacketPriority priority, PacketReliability reliability, byte orderingChannel = 0)
		{
			if (networkManager == null)
				networkManager = networkManagerFactory();

			networkManager.Broadcast(id, data, excludedAddress, priority, reliability, orderingChannel);
		}
	}
}
