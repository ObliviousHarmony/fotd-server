using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    /// <summary>
    /// A reference to a packet of a specific type.
    /// </summary>
    /// <remarks>
    /// Using this allows us to have raw buffers for packets and
    /// avoid allocating lots of memory for each individually.
    /// </remarks>
    public struct PacketRef : IDisposable
    {
        public readonly PacketIdentifier ID;
        public readonly NetworkAddress Sender;
        private readonly Memory<byte> _data;
        private readonly PacketBuffer _parentBuffer;
        private int _disposed;

        public PacketRef(
            PacketIdentifier id,
            NetworkAddress sender,
            Memory<byte> data,
            PacketBuffer parentBuffer
        )
        {
            ID = id;
            Sender = sender;
            _data = data;
            _parentBuffer = parentBuffer;
        }

        public ref TPacket Data<TPacket>() where TPacket : unmanaged
        {
            if (Volatile.Read(ref _disposed) == 1)
                throw new ObjectDisposedException(nameof(PacketRef));

            if (!PacketHelpers.IsPacketOfType<TPacket>(ID))
                throw new InvalidOperationException($"PacketRef does not contain data of type {typeof(TPacket)}");

            return ref MemoryMarshal.AsRef<TPacket>(_data.Span);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            _parentBuffer.Free(this);
        }
    }
}
