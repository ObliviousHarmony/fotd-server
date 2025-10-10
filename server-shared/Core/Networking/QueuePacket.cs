using System.Buffers;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    public struct QueuePacket : IDisposable
    {
        public readonly PacketIdentifier ID;
        public readonly PacketPriority Priority;
        public readonly PacketReliability Reliability;
        public readonly byte OrderingChannel;

        private readonly byte[] _packetData;
        private readonly NetworkAddress _networkAddress;
        private readonly List<NetworkAddress>? _networkAddresses;

        private bool _broadcast;
        private int _disposed;

        public QueuePacket(
            PacketIdentifier id,
            byte[] packetData,
            NetworkAddress networkAddress,
            List<NetworkAddress>? networkAddresses,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel
        )
        {
            ID = id;
            _packetData = packetData;
            _networkAddress = networkAddress;
            _networkAddresses = networkAddresses;
            Priority = priority;
            Reliability = reliability;
            OrderingChannel = orderingChannel;
        }

        public readonly ReadOnlySpan<byte> Data
        {
            get
            {
                if (Volatile.Read(in _disposed) != 0)
                    throw new ObjectDisposedException(nameof(QueuePacket));

                return _packetData.AsSpan(0, PacketHelpers.GetPacketSize(ID));
            }
        }

        public readonly ReadOnlySpan<NetworkAddress> NetworkAddresses
        {
            get
            {
                if (Volatile.Read(in _disposed) != 0)
                    throw new ObjectDisposedException(nameof(QueuePacket));

                // Make sure to consider any addresses that have been removed.
                if (_networkAddresses != null)
                    return CollectionsMarshal.AsSpan(_networkAddresses);

                return MemoryMarshal.CreateReadOnlySpan(in _networkAddress, 1);
            }
        }

        public readonly bool IsBroadcast
        {
            get
            {
                if (Volatile.Read(in _disposed) != 0)
                    throw new ObjectDisposedException(nameof(QueuePacket));

                return _broadcast;
            }
        }

        /// <summary>
        /// Creates a new instance of the packet that is marked for broadcast.
        /// </summary>
        /// <remarks>
        /// In order to avoid a defensive copy, this method is marked as readonly.
        /// As a consequence, after the new packet is constructed, the original
        /// will still exist and can be used to access the buffer. This is
        /// expected and is something that developers will need to avoid.
        /// </remarks>
        public readonly QueuePacket ForBroadcast()
        {
            if (_broadcast == true)
                return this;

            // Copy the struct so we can modify it.
            var modified = this;

            // Mark that the packet is a broadcast.
            modified._broadcast = true;

            return modified;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            ArrayPool<byte>.Shared.Return(_packetData);
        }
    }
}
