using FOMServer.Shared.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Packets
{
	/// <summary>
	/// Represents an error encountered while processing a packet.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ReadPacketError
	{
		public enum ReadErrorCode : byte
		{
			ERROR_MISSING_PACKET_ID,
			ERROR_UNHANDLED_PACKET_ID,
			ERROR_DESERIALIZATION
		}

		public PacketIdentifier OffendingID;
		public ReadErrorCode ErrorCode;
	}
}
