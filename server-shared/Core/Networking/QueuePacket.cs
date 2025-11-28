using System.Buffers;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

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
        public readonly PacketIdentifier ID;
        public readonly PacketPriority Priority;
        public readonly PacketReliability Reliability;
        public readonly byte OrderingChannel;
        public readonly bool IsBroadcast;

        private readonly byte[] _packetData;
        private readonly NetworkAddress _networkAddress;
        private readonly List<NetworkAddress>? _networkAddresses;

        public QueuePacket(
            PacketIdentifier id,
            byte[] packetData,
            NetworkAddress networkAddress,
            List<NetworkAddress>? networkAddresses,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel,
            bool broadcast = false
        )
        {
            ID = id;
            _packetData = packetData;
            _networkAddress = networkAddress;
            _networkAddresses = networkAddresses;
            Priority = priority;
            Reliability = reliability;
            OrderingChannel = orderingChannel;
            IsBroadcast = broadcast;
        }

        /// <summary>
        /// Copy constructor with broadcast override.
        /// </summary>
        private QueuePacket(in QueuePacket other, bool broadcast)
        {
            ID = other.ID;
            _packetData = other._packetData;
            _networkAddress = other._networkAddress;
            _networkAddresses = other._networkAddresses;
            Priority = other.Priority;
            Reliability = other.Reliability;
            OrderingChannel = other.OrderingChannel;
            IsBroadcast = broadcast;
        }

        public ReadOnlySpan<byte> Data => _packetData.AsSpan(0, PacketHelpers.GetPacketSize(ID));

        public ReadOnlySpan<NetworkAddress> NetworkAddresses
        {
            get
            {
                if (_networkAddresses != null)
                    return CollectionsMarshal.AsSpan(_networkAddresses);

                return MemoryMarshal.CreateReadOnlySpan(in _networkAddress, 1);
            }
        }

        /// <summary>
        /// Returns a copy of this packet with the broadcast flag set.
        /// </summary>
        /// <remarks>
        /// The returned packet shares the same underlying buffer. Only one
        /// instance should be disposed (typically by the NetworkManager).
        /// </remarks>
        public QueuePacket WithBroadcast() => IsBroadcast ? this : new QueuePacket(in this, broadcast: true);

        /// <summary>
        /// Returns the packet data buffer to the array pool.
        /// </summary>
        /// <remarks>
        /// This should only be called once per buffer, typically by the
        /// NetworkManager after the packet has been sent.
        /// </remarks>
        public void Release()
        {
            ArrayPool<byte>.Shared.Return(_packetData);
        }
    }
}
