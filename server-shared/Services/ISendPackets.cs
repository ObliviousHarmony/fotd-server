using FOMServer.Shared.Enums;
using FOMServer.Shared.Models;

namespace FOMServer.Shared.Services
{
	/// <summary>
	/// Describes a service that can send packets over the network.
	/// </summary>
	public interface ISendPackets
	{
		/// <summary>
		/// Sends a packet over the network.
		/// </summary>
		void Send(
			PacketIdentifier id,
			FOMData data,
			NetworkAddress destination,
			PacketPriority priority,
			PacketReliability reliability,
			byte orderingChannel = 0
		);

		/// <summary>
		/// Broadcast a packet to all connected clients.
		/// </summary>
        void Broadcast(
			PacketIdentifier id,
			FOMData data,
			NetworkAddress excludedAddress,
			PacketPriority priority,
			PacketReliability reliability,
			byte orderingChannel = 0
		);
    }
}
