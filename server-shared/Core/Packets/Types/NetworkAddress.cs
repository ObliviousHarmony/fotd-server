using System.Net;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NetworkAddress
    {
        public static readonly NetworkAddress Unassigned = new NetworkAddress
        {
            Address = "255.255.255.255",
            Port = 0xFFFF
        };

        private uint _binaryAddress;
        public ushort Port;

        public string Address
        {
            readonly get
            {
                // _binaryAddress stores bytes in network order (same as inet_addr)
                return string.Join(".", BitConverter.GetBytes(_binaryAddress));
            }
            set
            {
                if (!IPAddress.TryParse(value, out var ip))
                    throw new ArgumentException("Invalid IP address format", nameof(value));

                var bytes = ip.GetAddressBytes();
                if (bytes.Length != 4)
                    throw new ArgumentException("Only IPv4 addresses are supported", nameof(value));

                // GetAddressBytes returns network order bytes, BitConverter gives us the
                // same uint value that inet_addr would return (bytes in memory = network order)
                _binaryAddress = BitConverter.ToUInt32(bytes, 0);
            }
        }

        public static bool operator ==(NetworkAddress a, NetworkAddress b) => a.Equals(b);
        public static bool operator !=(NetworkAddress a, NetworkAddress b) => !a.Equals(b);

        public override readonly bool Equals(object? obj)
        {
            if (obj is not NetworkAddress other)
                return false;
            return _binaryAddress == other._binaryAddress && Port == other.Port;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(_binaryAddress, Port);
        }

        public override readonly string ToString()
        {
            return $"{Address}:{Port}";
        }
    }
}
