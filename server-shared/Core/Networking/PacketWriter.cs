using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Shared.Core.Networking
{
    public struct PacketWriter<TPacket> : IDisposable where TPacket : unmanaged
    {
        private int _addressCount;
        private NetworkAddress _networkAddress;
        private NetworkAddress[]? _networkAddresses;
        private PacketPriority _priority;
        private PacketReliability _reliability;
        private byte _orderingChannel;
        private bool _broadcast;

        private readonly int _packetSize;

        /// <summary>
        /// Rather than trying to hold onto a TPacket instance directly, we use a
        /// raw buffer that is sized to hold the packet type. This allows us to
        /// avoid unnecessary allocations and copying when building packets.
        /// </summary>
        private readonly byte[] _packetData;

        /// <summary>
        /// Once the writer has been used to build a packet, this flag
        /// indicates that it no longer owns the buffer and should not
        /// return it to the pool when disposed.
        /// </summary>
        private int _ownsBuffer;

        public PacketWriter()
        {
            // Since most packets are sent to a single address, we optimize for that case
            // by having a single address field. When more addresses are needed, an
            // array can be set and used in place of the single address.
            _addressCount = 0;
            _networkAddress = NetworkAddress.Unassigned;
            _networkAddresses = null;

            _priority = PacketPriority.Medium;
            _reliability = PacketReliability.ReliableOrdered;
            _orderingChannel = 0;

            // In order to keep the packet as a `readonly struct`, we need to declare whether
            // or not a packet is for broadcast before building. Since direct packets will
            // always require a call to add a destination, we can default to broadcast
            // to avoid an extra call for broadcast packets.
            _broadcast = true;

            // Since packets are generally small and very short-lived, we will
            // use the shared array pool to avoid excessive allocations.
            _packetSize = PacketHelpers.GetPacketSize<TPacket>();
            _packetData = ArrayPool<byte>.Shared.Rent(_packetSize);
            _ownsBuffer = 1;

            // Make sure there's no junk data in the buffer.
            Unsafe.InitBlock(ref _packetData[0], 0, (uint)_packetSize);
        }

        public PacketWriter(in NetworkAddress destination) : this()
        {
            AddDestination(in destination);
        }

        public PacketWriter(bool broadcast, in NetworkAddress destinationOrExcludeAddress) : this()
        {
            _broadcast = broadcast;
            if (_broadcast)
                ExcludeFromBroadcast(in destinationOrExcludeAddress);
            else
                AddDestination(in destinationOrExcludeAddress);
        }

        public ref TPacket Data
        {
            get
            {
                ThrowIfBuilt();
                return ref MemoryMarshal.AsRef<TPacket>(_packetData.AsSpan());
            }
        }

        public PacketPriority Priority
        {
            get => _priority;
            set
            {
                ThrowIfBuilt();
                _priority = value;
            }
        }

        public PacketReliability Reliability
        {
            get => _reliability;
            set
            {
                ThrowIfBuilt();
                _reliability = value;
            }
        }

        public byte OrderingChannel
        {
            get => _orderingChannel;
            set
            {
                ThrowIfBuilt();
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
            ThrowIfBuilt();

            // A broadcast packet is considered explicit if it has an exclusion address.
            if (_broadcast && _networkAddress != NetworkAddress.Unassigned)
                throw new InvalidOperationException("Cannot add destinations after calling ExcludeFromBroadcast");

            if (_addressCount >= QueuePacket.MaxNetworkAddressesPerPacket)
                throw new InvalidOperationException($"Cannot add more than {QueuePacket.MaxNetworkAddressesPerPacket} destinations");

            // Once an address has been added, a packet can no longer be broadcasted.
            _broadcast = false;

            // Just keep adding addresses if we have already added more than one.
            if (_networkAddresses is not null)
            {
                TryGrowAddressArray();
                _networkAddresses[_addressCount++] = address;
                return;
            }

            if (_networkAddress == NetworkAddress.Unassigned)
            {
                _networkAddress = address;
                _addressCount = 1;
                return;
            }

            // Now that we have more than one address, rent a pooled array.
            _networkAddresses = ArrayPool<NetworkAddress>.Shared.Rent(32);
            _networkAddresses[0] = _networkAddress;
            _networkAddresses[1] = address;
            _addressCount = 2;
        }

        /// <summary>
        /// Sets the packet to broadcast mode with an optional exclusion address.
        /// </summary>
        public void ExcludeFromBroadcast(in NetworkAddress address)
        {
            ThrowIfBuilt();

            if (!_broadcast)
                throw new InvalidOperationException("Packet has a destination and cannot be broadcasted");

            if (_networkAddress != NetworkAddress.Unassigned)
                throw new InvalidOperationException("Packet can only exclude a single address from the broadcast");

            _networkAddress = address;
            _addressCount = 1;
        }

        public QueuePacket Build()
        {
            // Mark that it can't be used anymore.
            if (Interlocked.Exchange(ref _ownsBuffer, 0) != 1)
                throw new ObjectDisposedException(nameof(PacketWriter<TPacket>));

            return new QueuePacket(
                PacketHelpers.GetPacketTypeID<TPacket>(),
                _packetData,
                _networkAddress,
                _networkAddresses,
                _addressCount,
                _priority,
                _reliability,
                _orderingChannel,
                _broadcast
            );
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _ownsBuffer, 0) != 1)
                return;

            ArrayPool<byte>.Shared.Return(_packetData);

            if (_networkAddresses is not null)
                ArrayPool<NetworkAddress>.Shared.Return(_networkAddresses);
        }

        private void TryGrowAddressArray()
        {
            if (_addressCount < _networkAddresses!.Length)
                return;

            // Custom bucket jumps to skip intermediate sizes we don't use.
            // After 512, fall back to standard doubling.
            int newSize = _addressCount < 128 ? 128
                        : _addressCount < 512 ? 512
                        : _addressCount * 2;

            var newArray = ArrayPool<NetworkAddress>.Shared.Rent(newSize);
            _networkAddresses.AsSpan(0, _addressCount).CopyTo(newArray);
            ArrayPool<NetworkAddress>.Shared.Return(_networkAddresses);
            _networkAddresses = newArray;
        }

        private void ThrowIfBuilt()
        {
            if (Volatile.Read(in _ownsBuffer) != 1)
                throw new InvalidOperationException("Packet cannot be modified after building");
        }
    }
}
