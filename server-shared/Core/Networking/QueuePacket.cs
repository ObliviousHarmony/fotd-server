using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    public struct QueuePacket : IDisposable
    {
        public ref struct PacketData<TPacket> : IDisposable where TPacket : unmanaged
        {
            private byte[] _data;

            /// <summary>
            /// Indicates whether or not the packet's data has been transferred
            /// to the packet queue for handling. When that happens, we can
            /// no longer safely access the data buffer.
            /// </summary>
            private int _transferred;

            /// <summary>
            /// Indicates whether or not the packet has been disposed.
            /// </summary>
            private int _disposed;

            public PacketData()
            {
                PacketIdentifier id = PacketHelpers.GetPacketTypeID<TPacket>();
                _data = ArrayPool<byte>.Shared.Rent(PacketHelpers.GetPacketSize(id));
            }

            public byte[] TransferData()
            {
                if (Volatile.Read(ref _disposed) != 0)
                    throw new ObjectDisposedException(nameof(PacketData<TPacket>));
                if (Interlocked.CompareExchange(ref _transferred, 1, 0) == 1)
                    throw new InvalidOperationException("Packet data has already been transferred");

                return _data;
            }

            public ref TPacket Data
            {
                get
                {
                    if (Volatile.Read(ref _transferred) == 1)
                        throw new InvalidOperationException("Packet data has already been transferred and cannot be accessed");
                    if (Volatile.Read(ref _disposed) != 0)
                        throw new ObjectDisposedException(nameof(PacketData<TPacket>));
                    return ref MemoryMarshal.AsRef<TPacket>(_data.AsSpan());
                }
            }

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                    return;

                if (Volatile.Read(ref _transferred) == 0)
                    ArrayPool<byte>.Shared.Return(_data);
            }
        }

        public readonly PacketPriority Priority;
        public readonly PacketReliability Reliability;
        public readonly byte OrderingChannel;
        public readonly bool Broadcast;
        private PacketIdentifier? _id;
        private byte[]? _buffer;
        private int _disposed;

        // Mutually exclusive, a packet either has a single destination or multiple.
        private NetworkAddress _networkAddress;
        private NetworkAddress[]? _networkAddresses;

        /// <summary>
        /// Creates a packet data structure for the specified packet type.
        /// </summary>
        public static PacketData<TPacket> Create<TPacket>() where TPacket : unmanaged
        {
            return new PacketData<TPacket>();
        }

        public QueuePacket(
            NetworkAddress networkAddress,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0,
            bool broadcast = false
        )
        {
            Priority = priority;
            Reliability = reliability;
            OrderingChannel = orderingChannel;
            Broadcast = broadcast;

            _networkAddress = networkAddress;
            _networkAddresses = null;
        }

        public QueuePacket(
            in NetworkAddress[] networkAddresses,
            PacketPriority priority,
            PacketReliability reliability,
            byte orderingChannel = 0,
            bool broadcast = false
        )
        {
            Priority = priority;
            Reliability = reliability;
            OrderingChannel = orderingChannel;
            Broadcast = broadcast;

            // Don't allocate a fresh array, instead, use a rented pool.
            _networkAddresses = ArrayPool<NetworkAddress>.Shared.Rent(networkAddresses.Length);
            networkAddresses.AsSpan().CopyTo(_networkAddresses);
        }

        public PacketIdentifier ID
        {
            get
            {
                if (Volatile.Read(ref _disposed) == 1)
                    throw new ObjectDisposedException(nameof(QueuePacket));
                if (!_id.HasValue)
                    throw new InvalidOperationException("Packet ID has not been set. Did you forget to call TransferData()?");

                return _id.Value;
            }
        }

        public byte[] Data
        {
            get
            {
                if (Volatile.Read(ref _disposed) == 1)
                    throw new ObjectDisposedException(nameof(QueuePacket));
                if (_buffer == null)
                    throw new InvalidOperationException("Packet data has already been transferred to the network library and can no longer be accessed");
                return _buffer;
            }
        }

        /// <summary>
        /// Given a packet data structure, transfers its underlying buffer into
        /// the queue packet for sending to the network library.
        /// </summary>
        public void TransferData<TPacket>(in PacketData<TPacket> packetData) where TPacket : unmanaged
        {
            if (Volatile.Read(ref _disposed) == 1)
                throw new ObjectDisposedException(nameof(QueuePacket));

            if (Volatile.Read(ref _buffer) != null)
                throw new InvalidOperationException("Queue packet already contains buffer data");

            _id = PacketHelpers.GetPacketTypeID<TPacket>();
            _buffer = packetData.TransferData();
        }

        public Span<NetworkAddress> NetworkAddresses()
        {
            if (Volatile.Read(ref _disposed) == 1)
                throw new ObjectDisposedException(nameof(QueuePacket));

            if (_networkAddresses != null)
                return _networkAddresses.AsSpan(0, _networkAddresses.Length);

            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in _networkAddress), 1);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            if (_buffer != null)
                ArrayPool<byte>.Shared.Return(_buffer);
            if (_networkAddresses != null)
                ArrayPool<NetworkAddress>.Shared.Return(_networkAddresses);
        }
    }
}
