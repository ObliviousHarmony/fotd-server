using System.Runtime.CompilerServices;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Infrastructure.FOMNetwork;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
    /// Manages the lifetime of packet data waiting to be sent over the network.
    /// </summary>
    /// <remarks>
    /// This class pre-allocates buffers on the Pinned Object Heap (POH) to avoid
    /// per-batch pinning overhead. Packet data and network addresses are copied
    /// into these buffers, allowing the original packet buffers to be released
    /// immediately.
    /// </remarks>
    internal class SendPacketBuffer
    {
        private readonly SendPacket[] _sendPackets;
        private readonly byte[] _packetData;
        private readonly NetworkAddress[] _networkAddresses;

        private int _packetDataOffset;
        private int _networkAddressOffset;
        private int _packetCount;

        public SendPacketBuffer()
        {
            // Allocate to the pinned object heap so that we don't need to pin the buffer when sending it to native code.
            _sendPackets = GC.AllocateArray<SendPacket>(IPacketService.MaxBufferedPackets, pinned: true);
            _packetData = GC.AllocateArray<byte>(IPacketService.MaxBufferedPackets * PacketHelpers.MaxPacketSize, pinned: true);
            _networkAddresses = GC.AllocateArray<NetworkAddress>(IPacketService.MaxBufferedPackets * QueuePacket.MaxNetworkAddressesPerPacket, pinned: true);
        }

        public bool CanAdd => _packetCount < IPacketService.MaxBufferedPackets;

        public bool HasBatch => _packetCount > 0;

        /// <summary>
        /// Adds a packet to the send buffer, copying its data and addresses.
        /// </summary>
        /// <remarks>
        /// The packet is released after its data is copied. Do not use the packet
        /// after calling this method.
        /// </remarks>
        public unsafe bool Add(in QueuePacket packet)
        {
            if (_packetCount >= IPacketService.MaxBufferedPackets)
            {
                throw new InvalidOperationException("Cannot add more packets to the buffer");
            }

            var packetSize = PacketHelpers.GetPacketSize(packet.ID);

            // Copy packet data into the POH buffer.
            fixed (byte* destPtr = &_packetData[_packetDataOffset])
            fixed (byte* srcPtr = packet.Data)
            {
                Unsafe.CopyBlockUnaligned(destPtr, srcPtr, (uint)packetSize);
            }

            // Copy network addresses into the POH buffer.
            var addresses = packet.NetworkAddresses;
            for (var i = 0; i < addresses.Length; i++)
            {
                _networkAddresses[_networkAddressOffset + i] = addresses[i];
            }

            // Build the SendPacket struct with pointers into the POH buffers.
            fixed (byte* dataPtr = &_packetData[_packetDataOffset])
            fixed (NetworkAddress* addrPtr = &_networkAddresses[_networkAddressOffset])
            {
                _sendPackets[_packetCount] = new SendPacket
                {
                    ID = packet.ID,
                    Data = (IntPtr)dataPtr,
                    NumNetworkAddresses = addresses.Length,
                    NetworkAddresses = (IntPtr)addrPtr,
                    Priority = packet.Priority,
                    Reliability = packet.Reliability,
                    OrderingChannel = packet.OrderingChannel,
                    Broadcast = (byte)(packet.Broadcast ? 1 : 0)
                };
            }

            _packetDataOffset += packetSize;
            _networkAddressOffset += addresses.Length;
            _packetCount++;

            // Release the original packet's buffers back to the pool.
            packet.Release();

            return true;
        }

        public ReadOnlySpan<SendPacket> GetBatch()
        {
            return _sendPackets.AsSpan(0, _packetCount);
        }

        public void Reset()
        {
            _packetDataOffset = 0;
            _networkAddressOffset = 0;
            _packetCount = 0;
        }
    }
}
