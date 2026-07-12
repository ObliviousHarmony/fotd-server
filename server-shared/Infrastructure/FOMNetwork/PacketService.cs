using System.Runtime.InteropServices;
using FOMServer.Shared.Application.Networking;
using FOMServer.Shared.Infrastructure.FOMNetwork;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Services.FOMNetwork
{
    internal partial class PacketService : IPacketService
    {
        [field: ThreadStatic]
        private static List<PacketBuffer> PacketBuffers => field ??= new List<PacketBuffer>(5);

        public ReadOnlySpan<PacketRef> Receive(IntPtr peer)
        {
            var received = FOMNetwork_ReceivePackets(peer);
            if (received.Count == 0)
            {
                return [];
            }

            // Pull from the packet buffer pool so that we don't have
            // to keep allocating new buffers for every packet batch.
            PacketBuffer? packetBuffer = null;
            byte[]? byteBuffer = null;
            foreach (var buffer in PacketBuffers)
            {
                byteBuffer = buffer.Rent(received);
                if (byteBuffer is null)
                {
                    continue;
                }

                packetBuffer = buffer;
                break;
            }
            if (packetBuffer is null)
            {
                packetBuffer = new PacketBuffer();
                byteBuffer = packetBuffer.Rent(received);
                PacketBuffers.Add(packetBuffer);
            }

            unsafe
            {
                fixed (byte* bufferPtr = byteBuffer)
                {
                    if (FOMNetwork_ProcessPackets(peer, received, bufferPtr, byteBuffer!.Length) != 0)
                    {
                        throw new InvalidOperationException("An critical error occurred attempting to process packets");
                    }
                }
            }

            return packetBuffer.GetPackets();
        }

        public void Send(IntPtr peer, ReadOnlySpan<SendPacket> packets)
        {
            if (packets.IsEmpty)
            {
                return;
            }

            unsafe
            {
                fixed (SendPacket* ptr = packets)
                {
                    var ret = FOMNetwork_Send(peer, ptr, packets.Length);
                    if (ret == -1)
                    {
                        throw new InvalidOperationException("No packets to send");
                    }
                    else if (ret == -2)
                    {
                        throw new InvalidOperationException("Broadcast specified with multiple network addresses");
                    }
                }
            }
        }

        public void CloseConnection(IntPtr peer, NetworkAddress address, bool sendDisconnectionNotification)
        {
            if (FOMNetwork_CloseConnection(peer, address, (byte)(sendDisconnectionNotification ? 1 : 0)) != 0)
            {
                throw new InvalidOperationException("No peer was provided");
            }
        }

        [LibraryImport("FOMNetwork")]
        private static partial ReceivedPackets FOMNetwork_ReceivePackets(IntPtr peer);

        [LibraryImport("FOMNetwork")]
        private static unsafe partial int FOMNetwork_ProcessPackets(
            IntPtr peer,
            ReceivedPackets received,
            byte* packetBuffer,
            int packetBufferLen
        );

        [LibraryImport("FOMNetwork")]
        private static unsafe partial int FOMNetwork_Send(IntPtr peer, SendPacket* packets, int count);

        [LibraryImport("FOMNetwork")]
        private static unsafe partial int FOMNetwork_CloseConnection(
            IntPtr peer,
            NetworkAddress address,
            byte sendDisconnectionNotification
        );
    }
}
