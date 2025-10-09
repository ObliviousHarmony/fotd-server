using System.Buffers;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    /// <summary>
    /// A buffer for holding packet data that has been
    /// received from the network library.
    /// </summary>
    public class PacketBuffer
    {
        private static readonly Meter s_meter = new("FOMServer.Networking.PacketBuffer", "1.0.0");
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
        private PacketIdentifier[]? _packetIDs;

        /// <summary>
        /// A buffer for holding onto packet references so that each usage of the buffer
        /// does not need to allocate a new array and use more heap memory.
        /// </summary>
        private readonly PacketRef[] _packetRefs = new PacketRef[IPacketService.MaxBufferedPackets];

        public unsafe byte[]? Rent(ReceivedPackets received)
        {
            if (Interlocked.CompareExchange(ref _allocated, 1, 0) != 0)
                return null;
            Interlocked.Increment(ref s_activeBufferCount);

            Console.WriteLine("Rental Start");

            _packetIDs = ArrayPool<PacketIdentifier>.Shared.Rent(received.Count);

            // Allocate a buffer large enough to hold all of the packets.
            _bufferSize = 0;
            for (byte i = 0; i < received.Count; i++)
            {
                _packetIDs[i] = received.Identifiers[i];
                _bufferSize += PacketHelpers.GetPacketSize(received.Identifiers[i]);
            }
            _buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);

            // Zero-initialize the buffer so our buffer
            // doesn't contain any garbage data.
            Unsafe.InitBlock(ref _buffer[0], 0, (uint)_bufferSize);

            // Let's create all of the packet references
            // here so that they're easy to access later.
            for (var i = 0; i < received.Count; i++)
            {
                var startIndex = GetPacketStart(i);
                _packetRefs[i] = new PacketRef(
                    _packetIDs![i],
                    received.Senders[i],
                    _buffer.AsMemory(startIndex, PacketHelpers.GetPacketSize(_packetIDs[i])),
                    this
                );
            }
            // Mark that the references are ready to be used.
            Volatile.Write(ref _refCount, received.Count);

            return _buffer;
        }

        public Span<PacketRef> GetPackets()
        {
            if (Volatile.Read(ref _allocated) != 1 || Volatile.Read(ref _refCount) == 0)
                throw new InvalidOperationException("PacketBuffer has not been allocated");

            Console.WriteLine();

            Console.WriteLine($"Raw Buffer Size: {_bufferSize}");
            for (int i = 0; i < _bufferSize; i++)
                Console.Write($"{_buffer![i]:X2} ");
            Console.WriteLine();

            var packets = _packetRefs.AsSpan(0, _refCount);
            for (int i = 0; i < packets.Length; i++)
                packets[i].PrintContent();

            return packets;
        }

        /// <summary>
        /// Frees the packet reference so that the buffer can be disposed.
        /// </summary>
        /// <remarks>
        /// Every PacketRef returned from this buffer MUST be freed in
        /// order for the buffer to be returned to the pool.
        /// </remarks>
        public void Free(in PacketRef refToFree)
        {
            if (Volatile.Read(ref _refCount) == 0)
                throw new InvalidOperationException("PacketBuffer has not been allocated");

            // We don't need the buffer anymore when all of the packets have been processed.
            var refCount = Interlocked.Decrement(ref _refCount);
            if (refCount == 0)
            {
                Console.WriteLine("Freed Buffer Start");
                var tempIDs = Interlocked.Exchange(ref _packetIDs, null);
                ArrayPool<PacketIdentifier>.Shared.Return(tempIDs!);

                var tempBuffer = Interlocked.Exchange(ref _buffer, null);
                ArrayPool<byte>.Shared.Return(tempBuffer!);

                Interlocked.Decrement(ref s_activeBufferCount);
                Interlocked.Exchange(ref _allocated, 0);
                Console.WriteLine("Freed Buffer Done");
            }
        }

        private int GetPacketStart(int index)
        {
            if (Volatile.Read(ref _allocated) != 1 || Volatile.Read(ref _buffer) == null)
                throw new InvalidOperationException("PacketBuffer has not been allocated");

            if (index >= _packetIDs!.Length)
                throw new IndexOutOfRangeException($"Packet {index} is out of range {_packetIDs.Length}");

            var currentIndex = 0;
            var offset = 0;
            while (currentIndex < index)
                offset += PacketHelpers.GetPacketSize(_packetIDs[currentIndex++]);

            var packetEnd = offset + PacketHelpers.GetPacketSize(_packetIDs[index]);
            if (packetEnd > _bufferSize)
            {
                throw new InvalidOperationException(
                    $"Packet {_packetIDs[index]} ({index}) Size {packetEnd - offset} Overflow - {_bufferSize}"
                );
            }

            return offset;
        }
    }
}
