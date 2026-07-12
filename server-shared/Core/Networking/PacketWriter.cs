using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Buffers;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Utilities;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Core.Networking
{
    public ref struct PacketWriter<TPacket> : IDisposable
        where TPacket : unmanaged
    {
        private readonly bool _initialized;
        private int _addressCount;
        private NetworkAddress _networkAddress;
        private NetworkAddress[]? _networkAddresses;
        private PacketPriority _priority;
        private PacketReliability _reliability;
        private byte _orderingChannel;

        private readonly int _packetSize;

        /// <summary>
        /// Rather than trying to hold onto a TPacket instance directly, we use a
        /// raw buffer that is sized to hold the packet type. This allows us to
        /// avoid unnecessary allocations and copying when building packets.
        /// </summary>
        private readonly PinnedBuffer _packetData;

        /// <summary>
        /// Once the writer has been used to build a packet, this flag
        /// indicates that it no longer owns the buffer and should not
        /// return it to the pool when disposed.
        /// </summary>
        private bool _ownsBuffer;

        public PacketWriter(in NetworkAddress destination)
        {
            // Since most packets are sent to a single address, we optimize for that case
            // by having a single address field. When more addresses are needed, an
            // array can be set and used in place of the single address.
            _addressCount = 1;
            _networkAddress = destination;
            _networkAddresses = null;

            _priority = PacketPriority.Medium;
            _reliability = PacketReliability.ReliableOrdered;
            _orderingChannel = 0;

            // Since packets are generally small and very short-lived, we will
            // use the shared array pool to avoid excessive allocations.
            _packetSize = PacketHelpers.GetPacketSize<TPacket>();
            _packetData = PinnedArrayPool.Shared.Rent(_packetSize);
            _ownsBuffer = true;

            // Make sure there's no junk data in the buffer.
            Unsafe.InitBlock(ref _packetData.Array[0], 0, (uint)_packetSize);

            _initialized = true;
        }

        public readonly ref TPacket Data
        {
            get
            {
                ThrowIfInvalid();
                return ref MemoryMarshal.AsRef<TPacket>(_packetData.AsSpan());
            }
        }

        public PacketPriority Priority
        {
            readonly get => _priority;
            set
            {
                ThrowIfInvalid();
                _priority = value;
            }
        }

        public PacketReliability Reliability
        {
            readonly get => _reliability;
            set
            {
                ThrowIfInvalid();
                _reliability = value;
            }
        }

        public byte OrderingChannel
        {
            readonly get => _orderingChannel;
            set
            {
                ThrowIfInvalid();
                _orderingChannel = value;
            }
        }

        /// <summary>
        /// Adds a destination address to the packet.
        /// </summary>
        /// <remarks>
        /// When there is only a single address, it is stored in a dedicated field.
        /// Once more addresses are added, a pooled array is rented to hold them.
        /// This lets us avoid allocation in most cases where only a single address
        /// is needed, and reuse arrays for multi-destination packets.
        /// </remarks>
        public void AddDestination(in NetworkAddress address)
        {
            ThrowIfInvalid();

            if (_addressCount >= QueuePacket.MaxNetworkAddressesPerPacket)
            {
                throw new InvalidOperationException(
                    $"Cannot add more than {QueuePacket.MaxNetworkAddressesPerPacket} destinations"
                );
            }

            // Just keep adding addresses if we have already added more than one.
            if (_networkAddresses is not null)
            {
                TryGrowAddressArray();
                _networkAddresses[_addressCount++] = address;
                return;
            }

            // Now that we have more than one address, rent a pooled array.
            _networkAddresses = ArrayPool<NetworkAddress>.Shared.Rent(32);
            _networkAddresses[0] = _networkAddress;
            _networkAddresses[1] = address;
            _addressCount = 2;
        }

        public QueuePacket Build()
        {
            ThrowIfInvalid();

            // Mark that it can't be used anymore.
            _ownsBuffer = false;
            return new QueuePacket(
                PacketHelpers.GetPacketTypeId<TPacket>(),
                _packetData,
                _networkAddress,
                _networkAddresses,
                _addressCount,
                _priority,
                _reliability,
                _orderingChannel
            );
        }

        public void Dispose()
        {
            if (!_ownsBuffer)
            {
                return;
            }

            _ownsBuffer = false;
            PinnedArrayPool.Shared.Return(in _packetData);

            if (_networkAddresses is not null)
            {
                ArrayPool<NetworkAddress>.Shared.Return(_networkAddresses);
            }
        }

        private void TryGrowAddressArray()
        {
            if (_addressCount < _networkAddresses!.Length)
            {
                return;
            }

            // Custom bucket jumps to skip intermediate sizes we don't use.
            // After 512, fall back to standard doubling.
            var newSize =
                _addressCount < 128 ? 128
                : _addressCount < 512 ? 512
                : _addressCount * 2;

            var newArray = ArrayPool<NetworkAddress>.Shared.Rent(newSize);
            _networkAddresses.AsSpan(0, _addressCount).CopyTo(newArray);
            ArrayPool<NetworkAddress>.Shared.Return(_networkAddresses);
            _networkAddresses = newArray;
        }

        private readonly void ThrowIfInvalid()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("The PacketWriter has not been initialized");
            }

            if (!_ownsBuffer)
            {
                throw new ObjectDisposedException(nameof(PacketWriter<>));
            }
        }
    }
}
