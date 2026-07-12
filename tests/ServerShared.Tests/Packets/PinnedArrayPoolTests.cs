using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Buffers;

namespace FOMServer.Shared.Tests.Packets
{
    public class PinnedArrayPoolTests
    {
        [Fact]
        public void Rent_ReturnsBufferAtLeastRequestedLength()
        {
            var pool = new PinnedArrayPool();

            var buffer = pool.Rent(1000);

            Assert.True(buffer.Array.Length >= 1000);
        }

        [Fact]
        public void Rent_RoundsUpToPowerOfTwoBucket()
        {
            var pool = new PinnedArrayPool();

            // 1000 rounds up to the 1024-byte bucket.
            Assert.Equal(1024, pool.Rent(1000).Array.Length);

            // An exact power of two stays in its own bucket.
            Assert.Equal(1024, pool.Rent(1024).Array.Length);
        }

        [Fact]
        public void RentReturnRent_ReusesTheSameArray()
        {
            var pool = new PinnedArrayPool();

            var first = pool.Rent(1024);
            var array = first.Array;
            pool.Return(in first);

            var second = pool.Rent(1024);

            Assert.Same(array, second.Array);
        }

        [Fact]
        public void Return_DerivesBucketFromArrayLength()
        {
            var pool = new PinnedArrayPool();

            // Rent with a non-power-of-two length, then return it. The pool must route
            // it back to the 1024 bucket using the array's length, so a 1024 rent reuses it.
            var rented = pool.Rent(1000);
            var array = rented.Array;
            pool.Return(in rented);

            Assert.Same(array, pool.Rent(1024).Array);
        }

        [Fact]
        public void Pointer_MatchesPinnedArrayAddress()
        {
            var pool = new PinnedArrayPool();

            var buffer = pool.Rent(4096);

            Assert.Equal(Marshal.UnsafeAddrOfPinnedArrayElement(buffer.Array, 0), buffer.Pointer);
        }

        [Fact]
        public void Pointer_RemainsStableAcrossGarbageCollection()
        {
            var pool = new PinnedArrayPool();

            var buffer = pool.Rent(4096);
            var before = buffer.Pointer;

            for (var i = 0; i < 3; i++)
            {
                GC.Collect(2, GCCollectionMode.Forced, blocking: true);
                GC.WaitForPendingFinalizers();
            }

            Assert.Equal(before, Marshal.UnsafeAddrOfPinnedArrayElement(buffer.Array, 0));
        }

        [Fact]
        public void Return_DefaultBuffer_DoesNotThrow()
        {
            var pool = new PinnedArrayPool();

            pool.Return(default);
        }

        [Fact]
        public void Return_BeyondBucketCapacity_DropsWithoutThrowing()
        {
            var pool = new PinnedArrayPool();

            // The largest bucket retains only a few buffers; returning more than that
            // must silently drop the overflow rather than throw.
            var buffers = new PinnedBuffer[8];
            for (var i = 0; i < buffers.Length; i++)
            {
                buffers[i] = pool.Rent(PinnedArrayPool.MaximumBufferLength);
            }

            foreach (var buffer in buffers)
            {
                pool.Return(in buffer);
            }
        }

        [Fact]
        public void ConcurrentRentReturn_DoesNotCorrupt()
        {
            var pool = new PinnedArrayPool();

            Parallel.For(
                0,
                8,
                _ =>
                {
                    for (var i = 0; i < 5000; i++)
                    {
                        var buffer = pool.Rent(1024);
                        Assert.True(buffer.Array.Length >= 1024);
                        pool.Return(in buffer);
                    }
                }
            );
        }
    }
}
