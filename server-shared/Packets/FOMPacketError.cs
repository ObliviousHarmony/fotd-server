using FOMServer.Shared.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Packets
{
	/// <summary>
	/// Represents an error encountered while processing a packet.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FOMPacketError
	{
		/// <summary>
		/// Represents error codes that indicate issues encountered while processing FOM packets.
		/// </summary>
		public enum FOMPacketErrorCode : byte
		{
			// Must match the enum in `fom-network/include/fom-network/FOMPacket.h`.
			ERROR_MISSING_PACKET_ID,
			ERROR_UNHANDLED_PACKET_ID,
			ERROR_DESERIALIZATION
		}

		public PacketIdentifier OffendingID;
		public FOMPacketErrorCode ErrorCode;
	}
}
