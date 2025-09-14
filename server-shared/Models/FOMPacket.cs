
using FOMServer.Shared.Enums;
using FOMServer.Shared.Packets;
using System.Runtime.InteropServices;

/**
 * This file contains all of the packet structures defined in `fom-network/include/fom-network/FOMPacket.h`.
 *
 * In order for the interop to work correctly and efficiently, ALL of them must:
 *
 * - Match the C++ structure's data type sizes and layout EXACTLY.
 * - Use only blittable types (no bools, no strings, no arrays, no reference types)
 * - Be marked with `[StructLayout(LayoutKind.Sequential, Pack = 1)]` to ensure no padding is added.
 */
namespace FOMServer.Shared.Models
{
	/// <summary>
	/// Represents a union of possible packet data types.
	/// </summary>
	/// <remarks>
	/// This structure uses explicit layout to allow overlapping fields, enabling it to store different kinds
	/// of packet data. Only one of the fields should be accessed at a time, as they share the same memory space.
	/// </remarks>
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct FOMData
	{
		[FieldOffset(0)] public FOMPacketError error;
	}

	/// <summary>
	/// The main structure for all packets.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FOMPacket
	{
		public PacketIdentifier ID;
		public NetworkAddress Sender;
		public FOMData Data;
	}
}
