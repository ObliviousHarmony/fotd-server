using PacketPosition = FOMServer.Shared.Core.Packets.Types.Position;
using PacketPositionRotation = FOMServer.Shared.Core.Packets.Types.PositionRotation;

namespace FOMServer.World.Core.World
{
    internal class ServerPosition
    {
        private readonly Lock _syncRoot = new();

        private short _x;
        private short _y;
        private short _z;
        private ushort _rotation;

        public short X
        {
            get
            {
                lock (_syncRoot)
                {
                    return _x;
                }
            }
        }

        public short Y
        {
            get
            {
                lock (_syncRoot)
                {
                    return _y;
                }
            }
        }

        public short Z
        {
            get
            {
                lock (_syncRoot)
                {
                    return _z;
                }
            }
        }

        public ushort Rotation
        {
            get
            {
                lock (_syncRoot)
                {
                    return _rotation;
                }
            }
        }

        public void ApplyUpdate(in PacketPosition p)
        {
            lock (_syncRoot)
            {
                _x = p.X;
                _y = p.Y;
                _z = p.Z;
            }
        }

        public void ApplyUpdate(in PacketPositionRotation p)
        {
            lock (_syncRoot)
            {
                _x = p.Pos.X;
                _y = p.Pos.Y;
                _z = p.Pos.Z;
                _rotation = p.Rot;
            }
        }

        public void WriteTo(ref PacketPosition p)
        {
            lock (_syncRoot)
            {
                p.X = _x;
                p.Y = _y;
                p.Z = _z;
            }
        }

        public void WriteTo(ref PacketPositionRotation p)
        {
            lock (_syncRoot)
            {
                p.Pos.X = _x;
                p.Pos.Y = _y;
                p.Pos.Z = _z;
                p.Rot = _rotation;
            }
        }

        public override string ToString()
        {
            lock (_syncRoot)
            {
                return $"X: {_x}, Y: {_y}, Z: {_z}, Rotation: {_rotation}";
            }
        }
    }
}
