using System.Buffers;
using FOMServer.Shared.Models;

namespace FOMServer.Shared.Services.FOMNetwork
{
	public interface IPacketService
	{
		/// <summary>
		/// The maximum number of packets that can be buffered at once.
		/// </summary>
		/// <remarks>
		/// Must match `fom-network/src/PacketAPI.cpp` MaxBufferedPackets.
		/// </remarks>
		public const int MaxBufferedPackets = 256;

		/// <summary>
		/// Polls the network interface for packets, parses them, and returns them in a memory buffer.
		/// </summary>
		/// <remarks>
		/// By returning a buffer we can avoid allocating new memory for the packets to be stored
		/// in after parsing. The returned buffer must NEVER be used after the next call to
		/// Receive, as it will be overwritten.
		/// </remarks>
		/// <param name="peer">The peer to receive packets using.</param>
		/// <returns>The buffer containing the received packets.</returns>
		Span<FOMPacket> Receive(IntPtr peer);

		/// <summary>
		/// Sends packets to the specified destinations.
		/// </summary>
		/// <param name="peer">The peer to send packets using.</param>
		/// <param name="packets">A buffer of packets to send.</param>
		void Send(IntPtr peer, Span<SendPacket> packets);
	}
}
