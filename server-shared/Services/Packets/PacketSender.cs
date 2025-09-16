using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;

namespace FOMServer.Shared.Services.Packets
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
