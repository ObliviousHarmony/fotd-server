using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.PacketData
{
	/// <summary>
	/// Represents an error encountered while processing a packet.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct LoginRequestReturn
	{
		public enum StatusCode : byte
		{
			LOGIN_REQUEST_INVALID_INFORMATION,
			LOGIN_REQUEST_SUCCESS,
			LOGIN_REQUEST_OUTDATED_CLIENT,
			LOGIN_REQUEST_ALREADY_LOGGED_IN
		}

		public StatusCode Status;
		public fixed byte Username[19];
	}
}
