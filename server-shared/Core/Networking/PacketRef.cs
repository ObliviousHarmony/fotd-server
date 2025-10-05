using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

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
        /// <summary>
        /// Keeps track of whether or not the buffer has been disposed.
        /// </summary>
        private int _disposed;

        /// <summary>
        /// The ID of the packet this references.
        /// </summary>
        public readonly PacketIdentifier ID;

        /// <summary>
        /// The sender of the packet this references.
        /// </summary>
        public readonly NetworkAddress Sender;

        /// <summary>
        /// The raw buffer for the packet this references.
        /// </summary>
        private readonly Memory<byte> _data;

        /// <summary>
        /// A reference to the buffer that this packet references.
        /// </summary>
        private readonly PacketBuffer _parentBuffer;

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
                throw new ObjectDisposedException("PacketRef");

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
