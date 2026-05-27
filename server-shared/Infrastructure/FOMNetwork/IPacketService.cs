using FOMServer.Shared.Application.Networking;

namespace FOMServer.Shared.Infrastructure.FOMNetwork
{
    public interface IPacketService
    {
        /// <summary>
        /// The maximum number of packets that can be buffered at once.
        /// </summary>
        /// <remarks>
        /// Must match `fom-network/src/PacketApi.cpp` MaxBufferedPackets.
        /// </remarks>
        const int MaxBufferedPackets = 64;

        /// <summary>
        /// Polls the network interface for packets, parses them, and returns them in a memory buffer.
        /// </summary>
        /// <remarks>
        /// By returning a buffer we can avoid allocating new memory for the packets to be stored
        /// in after parsing. The returned buffer must NEVER be used after the next call to
        /// Receive, as it will be overwritten.
        /// </remarks>
        /// <returns>The buffer containing the received packets.</returns>
        ReadOnlySpan<PacketRef> Receive(IntPtr peer);

        /// <summary>
        /// Sends packets to the specified destinations.
        /// </summary>
        void Send(IntPtr peer, ReadOnlySpan<SendPacket> packets);
    }
}
