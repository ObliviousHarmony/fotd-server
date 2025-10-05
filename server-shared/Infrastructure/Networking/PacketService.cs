using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Networking;

namespace FOMServer.Shared.Services.FOMNetwork
{
    public partial class PacketService : IPacketService
    {
        /// <summary>
        /// A static pool for packet buffers to hold packets that have been received.
        /// </summary>
        /// <remarks>
        /// We are dealing with variable-sized packets that are made of different structs. Our PacketBuffer and PacketRef structs
        /// allow us to manage raw buffers while presenting a type-safe interface to the rest of the application.
        /// </remarks>
        private static readonly ConcurrentBag<PacketBuffer> s_packetBuffers = new ConcurrentBag<PacketBuffer>();

        public Span<PacketRef> Receive(IntPtr peer)
        {
            var received = FOMNetwork_ReceivePackets(peer);
            if (received.Count == 0)
                return Span<PacketRef>.Empty;

            // Pull from the packet buffer pool so that we don't have
            // to keep allocating new buffers for every packet batch.
            PacketBuffer? packetBuffer = null;
            byte[]? byteBuffer = null;
            foreach (var buffer in s_packetBuffers)
            {
                byteBuffer = buffer.Rent(received);
                if (byteBuffer == null)
                    continue;

                packetBuffer = buffer;
                break;
            }
            if (packetBuffer == null)
            {
                packetBuffer = new PacketBuffer();
                byteBuffer = packetBuffer.Rent(received);
                s_packetBuffers.Add(packetBuffer);
            }

            unsafe
            {
                fixed (byte* bufferPtr = byteBuffer)
                {
                    if (FOMNetwork_ProcessPackets(peer, received, bufferPtr, byteBuffer!.Length) != 0)
                        throw new InvalidOperationException("An critical error occurred attempting to process packets");
                }
            }

            return packetBuffer.GetPackets();
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
        private static unsafe partial int FOMNetwork_ProcessPackets(IntPtr peer, ReceivedPackets received, byte* packetBuffer, int packetBufferLen);

        [LibraryImport("FOMNetwork")]
        private static unsafe partial int FOMNetwork_Send(IntPtr peer, SendPacket* packets, int count);
    }
}
