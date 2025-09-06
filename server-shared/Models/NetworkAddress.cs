using System.Runtime.InteropServices;

namespace FOMServer.Shared.Models
{
	/// <summary>
	/// Represents the IP address and port of a packet source or destination.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NetworkAddress
	{
		public uint Address;
		public short Port;
	}
}
