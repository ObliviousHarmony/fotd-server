using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models
{
    /// <summary>
    /// Represents the IP address and port of a packet source or destination.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NetworkAddress
    {
        public uint Address;
        public ushort Port;

        public override readonly string ToString()
        {
            string ipString = string.Join(".", BitConverter.GetBytes(Address));
            return $"{ipString}:{Port}";
        }
    }
}
