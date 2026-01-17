using System.Buffers;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;

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

        public readonly PacketIdentifier ID;
        public readonly PacketPriority Priority;
        public readonly PacketReliability Reliability;
        public readonly byte OrderingChannel;
        public readonly bool Broadcast;

        private readonly byte[] _packetData;
        private readonly NetworkAddress _networkAddress;
        private readonly NetworkAddress[]? _networkAddresses;
        private readonly int _addressCount;

        public QueuePacket(
            PacketIdentifier id,
            byte[] packetData,
            NetworkAddress networkAddress,
            NetworkAddress[]? networkAddresses,
            int addressCount,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel,
            bool broadcast
        )
        {
            ID = id;
            _packetData = packetData;
            _networkAddress = networkAddress;
            _networkAddresses = networkAddresses;
            _addressCount = addressCount;
            Priority = priority;
            Reliability = reliability;
            OrderingChannel = orderingChannel;
            Broadcast = broadcast;
        }

        public ReadOnlySpan<byte> Data => _packetData.AsSpan(0, PacketHelpers.GetPacketSize(ID));

        public ReadOnlySpan<NetworkAddress> NetworkAddresses
        {
            get
            {
                if (_networkAddresses != null)
                    return _networkAddresses.AsSpan(0, _addressCount);

                return MemoryMarshal.CreateReadOnlySpan(in _networkAddress, 1);
            }
        }

        /// <summary>
        /// Returns the packet data buffer and address array to their pools.
        /// </summary>
        /// <remarks>
        /// This should only be called once per packet, typically by the
        /// NetworkManager after the packet has been sent.
        /// </remarks>
        public void Release()
        {
            ArrayPool<byte>.Shared.Return(_packetData);

            if (_networkAddresses != null)
                ArrayPool<NetworkAddress>.Shared.Return(_networkAddresses);
        }
    }
}
