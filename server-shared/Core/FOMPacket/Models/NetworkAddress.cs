using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Models
{
    /// <summary>
    /// Represents the IP address and port of a packet source or destination.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NetworkAddress
    {
        public static readonly NetworkAddress Unassigned = new NetworkAddress
        {
            Address = 0xFFFFFFFF,
            Port = 0xFFFF
        };

        public uint Address;
        public ushort Port;

        public override readonly bool Equals(object? obj)
        {
            if (obj is not NetworkAddress other)
                return false;
            return Address == other.Address && Port == other.Port;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Address, Port);
        }

        public override readonly string ToString()
        {
            string ipString = string.Join(".", BitConverter.GetBytes(Address));
            return $"{ipString}:{Port}";
        }
    }
}
