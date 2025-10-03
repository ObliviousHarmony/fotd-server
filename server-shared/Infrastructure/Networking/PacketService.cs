using System.Runtime.InteropServices;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.Networking;

namespace FOMServer.Shared.Services.FOMNetwork
{
    public partial class PacketService : IPacketService
    {
        /// <summary>
        /// A static buffer for receiving packets to avoid allocations.
        /// </summary>
        /// <remarks>
        /// In order for this buffer to be safe to use, the returned span MUST not be used after
        /// the next call to Receive, as it will be overwritten.
        /// </remarks>
        private static readonly Packet[] s_receiveBuffer = new Packet[IPacketService.MaxBufferedPackets];

        public Span<Packet> Receive(IntPtr peer)
        {
            var received = FOMNetwork_ReceivePackets(peer);
            if (received.Count == 0)
                return Span<Packet>.Empty;

            unsafe
            {
                fixed (Packet* bufferPtr = s_receiveBuffer)
                {
                    if (FOMNetwork_ProcessPackets(peer, received, bufferPtr, received.Count) != 0)
                        return Span<Packet>.Empty;
                }
            }

            return s_receiveBuffer.AsSpan(0, received.Count);
        }

        public void Send(IntPtr peer, Span<SendPacket> packets)
        {
            if (packets.IsEmpty)
                return;

            unsafe
            {
                fixed (SendPacket* ptr = packets)
                {
                    FOMNetwork_Send(peer, ptr, packets.Length);
                }
            }
        }

        [LibraryImport("FOMNetwork")]
        private static partial ReceivedPackets FOMNetwork_ReceivePackets(IntPtr peer);

        [LibraryImport("FOMNetwork")]
        private static unsafe partial int FOMNetwork_ProcessPackets(IntPtr peer, ReceivedPackets received, Packet* packetBuffer, int packetBufferLen);

        [LibraryImport("FOMNetwork")]
        private static unsafe partial int FOMNetwork_Send(IntPtr peer, SendPacket* packets, int count);
    }
}
