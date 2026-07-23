using System.Net;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NetworkAddress
    {
        public static readonly NetworkAddress Unassigned = new() { BinaryAddress = 0xFFFFFFFF, Port = 0xFFFF };

        public uint BinaryAddress;
        public ushort Port;

        public string Address
        {
            readonly get =>
                // BinaryAddress stores bytes in network order (same as inet_addr)
                string.Join(".", BitConverter.GetBytes(BinaryAddress));
            set
            {
                if (!IPAddress.TryParse(value, out var ip))
                {
                    throw new ArgumentException("Invalid IP address format", nameof(value));
                }

                var bytes = ip.GetAddressBytes();
                if (bytes.Length != 4)
                {
                    throw new ArgumentException("Only IPv4 addresses are supported", nameof(value));
                }

                // GetAddressBytes returns network order bytes, BitConverter gives us the
                // same uint value that inet_addr would return (bytes in memory = network order)
                BinaryAddress = BitConverter.ToUInt32(bytes, 0);
            }
        }

        public static bool operator ==(NetworkAddress a, NetworkAddress b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(NetworkAddress a, NetworkAddress b)
        {
            return !a.Equals(b);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is NetworkAddress other && BinaryAddress == other.BinaryAddress && Port == other.Port;
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
