using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Buffers
{
    /// <summary>
    /// A handle to a byte buffer rented from a <see cref="PinnedArrayPool"/>.
    /// </summary>
    /// <remarks>
    /// The backing array is allocated on the Pinned Object Heap, so it never moves
    /// and <see cref="Pointer"/> is valid for the array's entire lifetime. This lets
    /// callers hand the pointer to native code without pinning it per call.
    /// </remarks>
    public readonly struct PinnedBuffer
    {
        /// <summary>
        /// The rented backing array. Lives on the Pinned Object Heap.
        /// </summary>
        public readonly byte[] Array;

        /// <summary>
        /// A stable pointer to the first element of <see cref="Array"/>, suitable
        /// for handing directly to native code.
        /// </summary>
        public readonly IntPtr Pointer;

        internal PinnedBuffer(byte[] array, IntPtr pointer)
        {
            Array = array;
            Pointer = pointer;
        }

        public Span<byte> AsSpan()
        {
            return Array.AsSpan();
        }
    }

    /// <summary>
    /// A bucketed array pool whose buffers are allocated on the Pinned Object Heap,
    /// so each rented buffer has a stable address that can be handed to native code
    /// without per-call pinning.
    /// </summary>
    /// <remarks>
    /// The structure mirrors the BCL's <c>ConfigurableArrayPool</c>: power-of-two
    /// size buckets, each a small stack guarded by a <see cref="SpinLock"/>. The
    /// single difference is that buffers come from <see cref="GC.AllocateArray{T}(int, bool)"/>
    /// with <c>pinned: true</c> rather than <c>new byte[]</c>. A single shared free
    /// list per bucket (rather than per-core sharding) is deliberate: rents and
    /// returns happen on different threads, so a shared list hands a returned buffer
    /// straight back to any renter instead of stranding it on the consumer's core.
    /// </remarks>
    public sealed class PinnedArrayPool
    {
        /// <summary>
        /// The shared, process-wide pool.
        /// </summary>
        public static readonly PinnedArrayPool Shared = new();

        /// <summary>
        /// The smallest bucket size; also the minimum length any rent rounds up to.
        /// </summary>
        public const int MinimumBufferLength = 16;

        /// <summary>
        /// The largest poolable size. Rents above this allocate an uncached pinned
        /// buffer; returns of such buffers are dropped.
        /// </summary>
        public const int MaximumBufferLength = 4 * 1024 * 1024;

        private static readonly Meter s_meter = new("FOMServer.Buffers.PinnedArrayPool");

        private static readonly Counter<long> s_allocations = s_meter.CreateCounter<long>(
            "fomserver.pinned_pool.allocations",
            description: "Pinned buffers allocated because the bucket was empty (a cache miss). Sustained nonzero values for a bucket mean its retention cap is too small for the working set.");

        private static readonly Counter<long> s_drops = s_meter.CreateCounter<long>(
            "fomserver.pinned_pool.drops",
            description: "Returned buffers dropped because the bucket was full. Pairs with allocations to confirm a bucket is thrashing.");

        private static readonly Counter<long> s_fallbackAllocations = s_meter.CreateCounter<long>(
            "fomserver.pinned_pool.fallback_allocations",
            description: "Uncached pinned allocations for rents larger than the largest bucket. Should stay at zero; a nonzero value means a packet outgrew MaximumBufferLength.");

        private readonly Bucket[] _buckets;

        public PinnedArrayPool()
        {
            var maxBucketIndex = SelectBucketIndex(MaximumBufferLength);
            _buckets = new Bucket[maxBucketIndex + 1];
            for (var i = 0; i < _buckets.Length; i++)
            {
                var bufferLength = MinimumBufferLength << i;
                _buckets[i] = new Bucket(bufferLength, GetRetentionCap(bufferLength));
            }
        }

        /// <summary>
        /// Rents a pinned buffer at least <paramref name="minimumLength"/> bytes long.
        /// </summary>
        public PinnedBuffer Rent(int minimumLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);

            if (minimumLength == 0)
            {
                return new PinnedBuffer([], IntPtr.Zero);
            }

            var index = SelectBucketIndex(minimumLength);
            if (index < _buckets.Length)
            {
                return _buckets[index].Rent();
            }

            // The requested array is too large for a bucket.
            s_fallbackAllocations.Add(1);
            return Allocate(minimumLength);
        }

        /// <summary>
        /// Returns a previously rented buffer to the pool. Foreign, oversized, or
        /// default buffers are silently dropped. Never throws.
        /// </summary>
        public void Return(in PinnedBuffer buffer)
        {
            var array = buffer.Array;
            if (array is null || array.Length == 0)
            {
                return;
            }

            var index = SelectBucketIndex(array.Length);
            if (index < _buckets.Length && _buckets[index].Length == array.Length)
            {
                _buckets[index].Return(in buffer);
            }

            // Otherwise the buffer didn't come from a bucket (oversized or foreign):
            // drop it and let the GC reclaim the pinned array.
        }

        /// <summary>
        /// Allocates a fresh pinned buffer and caches its stable data pointer.
        /// </summary>
        private static unsafe PinnedBuffer Allocate(int length)
        {
            var array = GC.AllocateArray<byte>(length, pinned: true);
            var pointer = (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(array));
            return new PinnedBuffer(array, pointer);
        }

        /// <summary>
        /// Maps a requested length to its power-of-two bucket index. Mirrors
        /// <c>System.Buffers.Utilities.SelectBucketIndex</c>.
        /// </summary>
        private static int SelectBucketIndex(int bufferSize)
        {
            return BitOperations.Log2(((uint)bufferSize - 1) | (MinimumBufferLength - 1)) - 3;
        }

        /// <summary>
        /// Per-bucket retention cap. Large buckets retain few buffers so a burst
        /// doesn't permanently inflate the Pinned Object Heap.
        /// </summary>
        private static int GetRetentionCap(int bufferLength)
        {
            if (bufferLength <= 4 * 1024)
            {
                return 50;
            }

            return bufferLength <= 256 * 1024 ? 16 : 4;
        }

        private sealed class Bucket
        {
            private readonly PinnedBuffer[] _buffers;
            private readonly KeyValuePair<string, object?> _metricTag;
            private SpinLock _lock;
            private int _count;

            internal Bucket(int bufferLength, int capacity)
            {
                Length = bufferLength;
                _buffers = new PinnedBuffer[capacity];
                _metricTag = new KeyValuePair<string, object?>("bucket_bytes", bufferLength);
                _lock = new SpinLock(Debugger.IsAttached);
            }

            internal int Length { get; }

            internal PinnedBuffer Rent()
            {
                var lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    if (_count > 0)
                    {
                        var buffer = _buffers[--_count];
                        _buffers[_count] = default;
                        return buffer;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit(false);
                    }
                }

                // The bucket was empty, so we have to allocate a new one.
                s_allocations.Add(1, _metricTag);
                return Allocate(Length);
            }

            internal void Return(in PinnedBuffer buffer)
            {
                var dropped = true;
                var lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    if (_count < _buffers.Length)
                    {
                        _buffers[_count++] = buffer;
                        dropped = false;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit(false);
                    }
                }

                // The buffer was dropped for the GC to reclaim.
                if (dropped)
                {
                    s_drops.Add(1, _metricTag);
                }
            }
        }
    }
}
