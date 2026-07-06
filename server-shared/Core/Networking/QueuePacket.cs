using System.Buffers;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Buffers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Utilities;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Core.Networking
{
    /// <summary>
    /// Represents a packet queued for sending over the network.
    /// </summary>
    /// <remarks>
    /// This is a readonly struct to prevent defensive copies. The packet data
    /// buffer is owned by whoever calls Release() - typically the NetworkManager
    /// after the packet has been sent. Do not hold references to this struct
    /// after passing it to the send queue.
    /// </remarks>
    public readonly struct QueuePacket
    {
        /// <summary>
        /// The maximum number of network addresses that a packet can hold.
        /// </summary>
        public const int MaxNetworkAddressesPerPacket = 5000;

        public readonly PacketIdentifier Id;
        public readonly PacketPriority Priority;
        public readonly PacketReliability Reliability;
        public readonly byte OrderingChannel;
        public readonly bool Broadcast;

        private readonly PinnedBuffer _packetData;
        private readonly NetworkAddress _networkAddress;
        private readonly NetworkAddress[]? _networkAddresses;
        private readonly int _addressCount;

        public QueuePacket(
            PacketIdentifier id,
            in PinnedBuffer packetData,
            NetworkAddress networkAddress,
            NetworkAddress[]? networkAddresses,
            int addressCount,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel,
            bool broadcast
        )
        {
            Id = id;
            _packetData = packetData;
            _networkAddress = networkAddress;
            _networkAddresses = networkAddresses;
            _addressCount = addressCount;
            Priority = priority;
            Reliability = reliability;
            OrderingChannel = orderingChannel;
            Broadcast = broadcast;
        }

        public ReadOnlySpan<byte> Data => _packetData.Array.AsSpan(0, PacketHelpers.GetPacketSize(Id));

        /// <summary>
        /// A stable pointer to the packet data, valid for the lifetime of the
        /// rented buffer. 
        /// </summary>
        public IntPtr DataPointer => _packetData.Pointer;

        public ReadOnlySpan<NetworkAddress> NetworkAddresses => _networkAddresses is not null
                    ? _networkAddresses.AsSpan(0, _addressCount)
                    : MemoryMarshal.CreateReadOnlySpan(in _networkAddress, 1);

        /// <summary>
        /// Returns the packet data buffer and address array to their pools.
        /// </summary>
        /// <remarks>
        /// This should only be called once per packet, typically by the
        /// NetworkManager after the packet has been sent.
        /// </remarks>
        public void Release()
        {
            PinnedArrayPool.Shared.Return(in _packetData);

            if (_networkAddresses is not null)
            {
                ArrayPool<NetworkAddress>.Shared.Return(_networkAddresses);
            }
        }
    }
}
