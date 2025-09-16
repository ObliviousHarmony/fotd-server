using FOMServer.Shared.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Packets
{
	/// <summary>
	/// Represents an error encountered while processing a packet.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct LoginRequestReturn
	{
		public enum Status : byte
		{
			// Must match the enum in `fom-network/include/fom-network/packets/LoginRequestReturnPacket.h`.
			LOGIN_REQUEST_INVALID_INFORMATION,
			LOGIN_REQUEST_SUCCESS,
			LOGIN_REQUEST_OUTDATED_CLIENT,
			LOGIN_REQUEST_ALREADY_LOGGED_IN
		}

		public Status status;
		public fixed byte username[19];
	}
}
