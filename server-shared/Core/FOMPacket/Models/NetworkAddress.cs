using System.Net;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NetworkAddress
    {
        public static readonly NetworkAddress Unassigned = new NetworkAddress
        {
            BinaryAddress = 0xFFFFFFFF,
            Port = 0xFFFF
        };

        public uint BinaryAddress;
        public ushort Port;

        public string Address
        {
            readonly get
            {
                return string.Join(".", BitConverter.GetBytes(BinaryAddress));
            }
            set
            {
                if (!IPAddress.TryParse(value, out var ip))
                    throw new ArgumentException("Invalid IP address format", nameof(value));

                var bytes = ip.GetAddressBytes();
                if (bytes.Length != 4)
                    throw new ArgumentException("Only IPv4 addresses are supported", nameof(value));

                BinaryAddress = BitConverter.ToUInt32(bytes, 0);
            }
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is not NetworkAddress other)
                return false;
            return BinaryAddress == other.BinaryAddress && Port == other.Port;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(BinaryAddress, Port);
        }

        public override readonly string ToString()
        {
            return $"{Address}:{Port}";
        }
    }
}
