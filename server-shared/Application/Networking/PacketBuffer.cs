using System.Buffers;
using System.Diagnostics.Metrics;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Infrastructure.FOMNetwork;

namespace FOMServer.Shared.Application.Networking
{
    /// <summary>
    /// A buffer for holding packet data that has been received from the network library.
    /// </summary>
    /// <remarks>
    /// Each packet slot in the buffer uses the format: [status byte][packet data].
    /// The status byte indicates whether deserialization succeeded or failed.
    /// </remarks>
    internal class PacketBuffer
    {
        private static readonly Meter s_meter = new("FOMServer.Networking.PacketBuffer");
        private static readonly ObservableGauge<int> s_buffersInUse =
            s_meter.CreateObservableGauge(
                "fomserver.packet_buffers_in_use",
                () => new Measurement<int>(Volatile.Read(in s_activeBufferCount))
            );

        private static int s_activeBufferCount;

        private int _allocated;
        private int _refCount;
        private byte[]? _buffer;
        private int _bufferSize;
        private PacketIdentifier[]? _packetIds;

        /// <summary>
        /// Keeps track of the current version of the buffer.
        /// </summary>
        /// <remarks>
        /// Tracking the version of the buffer allows us to ensure that stale
        /// references aren't able to access or dispose of indices that
        /// have been re-allocated from the buffer being re-used.
        /// </remarks>
        private int _bufferVersion;

        /// <summary>
        /// A buffer for holding onto packet references so that each usage of the buffer
        /// does not need to allocate a new array and use more heap memory.
        /// </summary>
        private readonly PacketRef[] _packetRefs = new PacketRef[IPacketService.MaxBufferedPackets];

        /// <summary>
        /// Keep track of which packet references have been disposed.
        /// </summary>
        /// <remarks>
        /// Since PacketRef might be copied, we can't rely on it maintaining its own disposed flag.
        /// We will track it here in the PacketBuffer and ensure it can only be freed once.
        /// </remarks>
        private readonly int[] _packetRefDisposalFlags = new int[IPacketService.MaxBufferedPackets];

        public unsafe byte[]? Rent(ReceivedPackets received)
        {
            if (Interlocked.Exchange(ref _allocated, 1) != 0)
            {
                return null;
            }

            _ = Interlocked.Increment(ref s_activeBufferCount);

            _packetIds = ArrayPool<PacketIdentifier>.Shared.Rent(received.Count);

            // Allocate a buffer large enough to hold all of the packets.
            // Each packet slot is 1 byte status + packet data.
            _bufferSize = 0;
            for (byte i = 0; i < received.Count; i++)
            {
                _packetIds[i] = received.Identifiers[i];
                _bufferSize += 1 + PacketHelpers.GetPacketSize(received.Identifiers[i]);
            }
            _buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);

            // Let's create all of the packet references
            // here so that they're easy to access later.
            var bufferVersion = Volatile.Read(in _bufferVersion);
            for (var i = 0; i < received.Count; i++)
            {
                _packetRefDisposalFlags[i] = 0;

                var startIndex = GetPacketStart(i);
                var packetSize = PacketHelpers.GetPacketSize(_packetIds![i]);
                _packetRefs[i] = new PacketRef(
                    i,
                    bufferVersion,
                    _packetIds[i],
                    received.Senders[i],
                    _buffer.AsMemory(startIndex, 1 + packetSize),
                    this
                );
            }

            // Mark that the references are ready to be used.
            Volatile.Write(ref _refCount, received.Count);

            return _buffer;
        }

        public ReadOnlySpan<PacketRef> GetPackets()
        {
            return Volatile.Read(in _allocated) != 1 || Volatile.Read(in _refCount) == 0
                ? throw new InvalidOperationException("PacketBuffer has not been allocated")
                : (ReadOnlySpan<PacketRef>)_packetRefs.AsSpan(0, _refCount);
        }

        /// <summary>
        /// Indicates whether or not the given packet has been disposed of.
        /// </summary>
        public bool IsPacketDisposed(ref readonly PacketRef packet)
        {
            return Volatile.Read(in _bufferVersion) != packet.BufferVersion || Volatile.Read(in _packetRefDisposalFlags[packet.RefIndex]) != 0;
        }

        /// <summary>
        /// Frees the packet reference so that the buffer can be disposed.
        /// </summary>
        /// <remarks>
        /// Every PacketRef returned from this buffer MUST be freed in
        /// order for the buffer to be returned to the pool.
        /// </remarks>
        public void DisposePacket(ref readonly PacketRef refToFree)
        {
            if (Volatile.Read(in _refCount) == 0)
            {
                throw new InvalidOperationException("PacketBuffer has not been allocated");
            }

            if (Volatile.Read(in _bufferVersion) != refToFree.BufferVersion)
            {
                throw new ObjectDisposedException(nameof(PacketRef));
            }

            ref var disposalFlag = ref _packetRefDisposalFlags[refToFree.RefIndex];
            var disposed = Interlocked.Exchange(ref disposalFlag, 1);
            if (disposed != 0)
            {
                throw new ObjectDisposedException(nameof(PacketRef));
            }

            // We don't need the buffer anymore when all of the packets have been processed.
            var refCount = Interlocked.Decrement(ref _refCount);
            if (refCount == 0)
            {
                // Invalidate stale references before freeing the buffers so
                // that they aren't able to access memory that has been
                // returned back to the pool.
                _ = Interlocked.Increment(ref _bufferVersion);

                var tempIds = Interlocked.Exchange(ref _packetIds, null);
                ArrayPool<PacketIdentifier>.Shared.Return(tempIds!);

                var tempBuffer = Interlocked.Exchange(ref _buffer, null);
                ArrayPool<byte>.Shared.Return(tempBuffer!);

                _ = Interlocked.Decrement(ref s_activeBufferCount);
                _ = Interlocked.Exchange(ref _allocated, 0);
            }
        }

        private int GetPacketStart(int index)
        {
            if (Volatile.Read(in _allocated) != 1 || Volatile.Read(in _buffer) is null)
            {
                throw new InvalidOperationException("PacketBuffer has not been allocated");
            }

            if (index >= _packetIds!.Length)
            {
                throw new IndexOutOfRangeException($"Packet {index} is out of range {_packetIds.Length}");
            }

            var currentIndex = 0;
            var offset = 0;
            while (currentIndex < index)
            {
                offset += 1 + PacketHelpers.GetPacketSize(_packetIds[currentIndex++]);
            }

            var packetEnd = offset + 1 + PacketHelpers.GetPacketSize(_packetIds[index]);
            return packetEnd > _bufferSize
                ? throw new InvalidOperationException(
                    $"Packet {_packetIds[index]} ({index}) Size {packetEnd - offset} Overflow - {_bufferSize}"
                )
                : offset;
        }
    }
}
