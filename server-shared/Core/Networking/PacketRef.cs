using System.Runtime.InteropServices;
using System.Text;
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
    public readonly struct PacketRef
    {
        public readonly int RefIndex;
        public readonly int BufferVersion;
        private readonly PacketIdentifier _id;
        private readonly NetworkAddress _sender;
        private readonly ReadOnlyMemory<byte> _data;
        private readonly PacketBuffer _parentBuffer;

        public PacketRef(
            int refIndex,
            int bufferVersion,
            PacketIdentifier id,
            NetworkAddress sender,
            in ReadOnlyMemory<byte> data,
            PacketBuffer parentBuffer
        )
        {
            RefIndex = refIndex;
            BufferVersion = bufferVersion;
            _id = id;
            _sender = sender;
            _data = data;
            _parentBuffer = parentBuffer;
        }

        public readonly PacketIdentifier ID
        {
            get
            {
                if (_parentBuffer.IsPacketDisposed(in this))
                    throw new ObjectDisposedException(nameof(PacketRef));
                return _id;
            }
        }

        public readonly NetworkAddress Sender
        {
            get
            {
                if (_parentBuffer.IsPacketDisposed(in this))
                    throw new ObjectDisposedException(nameof(PacketRef));
                return _sender;
            }
        }

        public readonly ref readonly TPacket Data<TPacket>() where TPacket : unmanaged
        {
            if (_parentBuffer.IsPacketDisposed(in this))
                throw new ObjectDisposedException(nameof(PacketRef));

            if (!PacketHelpers.IsPacketOfType<TPacket>(_id))
                throw new InvalidOperationException($"PacketRef does not contain data of type {typeof(TPacket)}");

            return ref MemoryMarshal.AsRef<TPacket>(_data.Span);
        }

        public void Dispose()
        {
            _parentBuffer.DisposePacket(in this);
        }

        public override readonly string ToString()
        {
            int packetSize = PacketHelpers.GetPacketSize(_id);
            var sb = new StringBuilder(32 + (packetSize * 3));

            sb.Append(_id);
            sb.Append(" [");
            sb.Append(packetSize);
            sb.Append(" bytes]: ");

            if (!_parentBuffer.IsPacketDisposed(in this))
            {
                // Append as hex pairs separated by spaces
                var data = _data.Span;
                for (int i = 0; i < packetSize; i++)
                {
                    sb.Append(data[i].ToString("X2"));
                    if (i < packetSize - 1)
                        sb.Append(' ');
                }
            }
            else
                sb.Append("<disposed>");

            return sb.ToString();
        }
    }
}
