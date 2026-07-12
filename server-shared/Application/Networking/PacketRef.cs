using System.Runtime.InteropServices;
using System.Text;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Utilities;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Application.Networking
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
        private readonly ReadOnlyMemory<byte> _data;
        private readonly PacketBuffer _parentBuffer;

        internal PacketRef(
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
            Sender = sender;
            _data = data;
            _parentBuffer = parentBuffer;
        }

        public readonly PacketIdentifier Id =>
            _parentBuffer.IsPacketDisposed(in this) ? throw new ObjectDisposedException(nameof(PacketRef)) : _id;

        public readonly NetworkAddress Sender =>
            _parentBuffer.IsPacketDisposed(in this) ? throw new ObjectDisposedException(nameof(PacketRef)) : field;

        public readonly SerializationStatus Status =>
            _parentBuffer.IsPacketDisposed(in this)
                ? throw new ObjectDisposedException(nameof(PacketRef))
                : (SerializationStatus)_data.Span[0];

        public readonly ref readonly TPacket Data<TPacket>()
            where TPacket : unmanaged
        {
            if (_parentBuffer.IsPacketDisposed(in this))
            {
                throw new ObjectDisposedException(nameof(PacketRef));
            }

            if (!PacketHelpers.IsPacketOfType<TPacket>(_id))
            {
                throw new InvalidOperationException($"PacketRef does not contain data of type '{typeof(TPacket)}'");
            }

            var data = _data.Span;

            var status = (SerializationStatus)data[0];
            if (status != SerializationStatus.Success)
            {
                throw new InvalidOperationException($"Cannot access data of packet with status {status}");
            }

            return ref MemoryMarshal.AsRef<TPacket>(data[1..]);
        }

        public void Dispose()
        {
            _parentBuffer.DisposePacket(in this);
        }

        public override readonly string ToString()
        {
            var packetSize = PacketHelpers.GetPacketSize(_id);
            var sb = new StringBuilder(32 + (packetSize * 3));

            sb.Append(_id);
            sb.Append(" [");
            sb.Append(packetSize);
            sb.Append(" bytes]: ");

            var data = _data.Span;
            var status = (SerializationStatus)data[0];
            if (status != SerializationStatus.Success)
            {
                sb.Append("<error: ");
                sb.Append(status.ToString());
                sb.Append('>');
            }
            else if (_parentBuffer.IsPacketDisposed(in this))
            {
                sb.Append("<disposed>");
            }
            else
            {
                // Append as hex pairs separated by spaces (skip status byte at index 0)
                for (var i = 1; i <= packetSize; i++)
                {
                    sb.Append(data[i].ToString("X2"));
                    if (i < packetSize)
                    {
                        sb.Append(' ');
                    }
                }
            }

            return sb.ToString();
        }
    }
}
