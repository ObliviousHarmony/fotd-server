using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.PacketData;
using System.Runtime.InteropServices;

/**
 * This file contains all of the packet structures defined in `fom-network/include/fom-network/FOMPacket.h`.
 *
 * In order for the interop to work correctly and efficiently, ALL of them must:
 *
 * - Match the C++ structure's data type sizes and layout EXACTLY.
 * - Use only blittable types (no bools, no strings, no arrays, no reference types)
 * - Be marked with `[StructLayout(LayoutKind.Sequential, Pack = 1)]` to ensure no padding is added.
 *
 * For every new packet, you must also update:
 *
 * - Packets/{PacketName}.cs: Requires a new struct definition.
 * - Packet struct added to the FOMData union below.
 * - Extensions/FOMPacketExtensions.cs: Requires a new FOMData type case.
 * - Server-Specific Handlers/<PacketName>Handler.cs: Requires a new packet handler implementation. Bind to IPacketHandler in server-specific CompositionRoot.cs.
 */
namespace FOMServer.Shared.Core.Models
{
	/// <summary>
	/// A placeholder struct for RakNet packet representation.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct RakNetPacket { }

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
		[FieldOffset(0)] public RakNetPacket rakNetPacket;
		[FieldOffset(0)] public ReadPacketError readError;
		[FieldOffset(0)] public LoginRequest loginRequest;
		[FieldOffset(0)] public LoginRequestReturn loginRequestReturn;
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
