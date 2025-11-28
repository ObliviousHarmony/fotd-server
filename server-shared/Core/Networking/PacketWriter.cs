using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    public struct PacketWriter<TPacket> : IDisposable where TPacket : unmanaged
    {
        private NetworkAddress _networkAddress;
        private List<NetworkAddress>? _networkAddresses;
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
        /// Once more addresses are added it will allocate a list to hold them.
        /// This lets us avoid the allocation in most cases where only a
        /// single address is needed for a packet.
        /// </remarks>
        public void AddDestination(NetworkAddress address)
        {
            ThrowIfBuilt();

            // A broadcast packet is considered explicit if it has an exclusion address.
            if (_broadcast && _networkAddress != NetworkAddress.Unassigned)
                throw new InvalidOperationException("Cannot add destinations after calling ExcludeFromBroadcast");

            // Once an address has been added, a packet can no longer be broadcasted.
            _broadcast = false;

            // Just keep adding addresses if we have already added more than one.
            if (_networkAddresses != null)
            {
                _networkAddresses.Add(address);
                return;
            }

            if (_networkAddress == NetworkAddress.Unassigned)
            {
                _networkAddress = address;
                return;
            }

            // Now that we have more than one address we need
            // to allocate a list to hold all of them.
            _networkAddresses = new List<NetworkAddress>(32)
            {
                _networkAddress,
                address
            };
        }

        /// <summary>
        /// Sets the packet to broadcast mode with an optional exclusion address.
        /// </summary>
        public void ExcludeFromBroadcast(NetworkAddress address)
        {
            ThrowIfBuilt();

            if (!_broadcast)
                throw new InvalidOperationException("Packet has a destination and cannot be broadcasted");

            if (_networkAddress != NetworkAddress.Unassigned)
                throw new InvalidOperationException("Packet can only exclude a single address from the broadcast");

            _networkAddress = address;
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
        }

        private void ThrowIfBuilt()
        {
            if (Volatile.Read(ref _ownsBuffer) != 1)
                throw new InvalidOperationException("Packet cannot be modified after building");
        }
    }
}
