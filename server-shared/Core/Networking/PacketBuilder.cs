using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    public struct PacketBuilder<TPacket> : IDisposable where TPacket : unmanaged
    {
        /// <summary>
        /// Rather than trying to hold onto a TPacket instance directly, we use a
        /// raw buffer that is sized to hold the packet type. This allows us to
        /// avoid unnecessary allocations and copying when building packets.
        /// </summary>
        private readonly byte[] _packetData;

        /// <summary>
        /// Once the builder has been used to build a packet, this flag
        /// indicates that it no longer owns the buffer and should not
        /// return it to the pool when disposed.
        /// </summary>
        private int _ownsBuffer;

        /// <summary>
        /// Since most packets are sent to a single address, we optimize for that case
        /// by having a single address field. When more addresses are needed, this
        /// array can be set and used in place of the single address.
        /// </summary>
        private List<NetworkAddress>? _networkAddresses;

        private readonly int _packetSize;
        private NetworkAddress _networkAddress;
        private PacketPriority _priority;
        private PacketReliability _reliability;
        private byte _orderingChannel;

        public PacketBuilder()
        {
            _packetSize = PacketHelpers.GetPacketSize<TPacket>();

            // Since packets are generally small and very short-lived, we will
            // use the shared array pool to avoid excessive allocations.
            _packetData = ArrayPool<byte>.Shared.Rent(_packetSize);
            _ownsBuffer = 1;

            // Make sure there's no junk data in the buffer.
            Unsafe.InitBlock(ref _packetData[0], 0, (uint)_packetSize);

            _networkAddress = NetworkAddress.Unassigned;
            _networkAddresses = null;
            _priority = PacketPriority.Medium;
            _reliability = PacketReliability.ReliableOrdered;
            _orderingChannel = 0;
        }

        public ref TPacket Data
        {
            get
            {
                if (Volatile.Read(ref _ownsBuffer) != 1)
                    throw new InvalidOperationException("Packet cannot be modified after building");

                return ref MemoryMarshal.AsRef<TPacket>(_packetData.AsSpan());
            }
        }

        /// <summary>
        /// Adds a network address to the packet.
        /// </summary>
        /// <remarks>
        /// When there is only a single address, it is stored in a dedicated field.
        /// Once more addresses are added it will allocate a list to hold them.
        /// This lets us avoid the allocation in most cases where only a
        /// single address is needed for a packet.
        /// </remarks>
        public PacketBuilder<TPacket> WithAddress(NetworkAddress address)
        {
            if (Volatile.Read(ref _ownsBuffer) != 1)
                throw new InvalidOperationException("Packet cannot be modified after building");

            // Just keep adding addresses if we have already added more than one.
            if (_networkAddresses != null)
            {
                _networkAddresses.Add(address);
                return this;
            }

            if (_networkAddress == NetworkAddress.Unassigned)
            {
                _networkAddress = address;
                return this;
            }

            // Now that we have more than one address we need
            // to allocate a list to hold all of them.
            _networkAddresses = new List<NetworkAddress>(16)
            {
                _networkAddress,
                address
            };

            return this;
        }

        /// <summary>
        /// Changes the priority of the packet we are building.
        /// </summary>
        public PacketBuilder<TPacket> WithPriority(PacketPriority priority)
        {
            if (Volatile.Read(ref _ownsBuffer) != 1)
                throw new InvalidOperationException("Packet cannot be modified after building");

            _priority = priority;
            return this;
        }

        /// <summary>
        /// Changes the reliability of the packet we are building.
        /// </summary>
        public PacketBuilder<TPacket> WithReliability(PacketReliability reliability)
        {
            if (Volatile.Read(ref _ownsBuffer) != 1)
                throw new InvalidOperationException("Packet cannot be modified after building");

            _reliability = reliability;
            return this;
        }

        /// <summary>
        /// Changes the ordering channel of the packet we are building.
        /// </summary>
        public PacketBuilder<TPacket> WithOrderingChannel(byte orderingChannel)
        {
            if (Volatile.Read(ref _ownsBuffer) != 1)
                throw new InvalidOperationException("Packet cannot be modified after building");

            _orderingChannel = orderingChannel;
            return this;
        }

        public QueuePacket Build()
        {
            // Mark that it can't be used anymore.
            if (Interlocked.Exchange(ref _ownsBuffer, 0) != 1)
                throw new ObjectDisposedException(nameof(PacketBuilder<TPacket>));

            return new QueuePacket(
                PacketHelpers.GetPacketTypeID<TPacket>(),
                _packetData,
                _networkAddress,
                _networkAddresses,
                _priority,
                _reliability,
                _orderingChannel
            );
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _ownsBuffer, 0) != 1)
                return;

            ArrayPool<byte>.Shared.Return(_packetData);
        }
    }
}
