using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.FOMPacket.Models;
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
 * - Core/Models/FOMData/{PacketName}.cs: Requires a new struct definition.
 * - Packet struct added to the FOMDataUnion union below.
 * - Extensions/FOMPacketExtensions.cs: Requires a new FOMDataUnion type case.
 * - Server-Specific PacketHandlers/<PacketName>Handler.cs: Requires a new packet handler implementation. Bind to IPacketHandler in server-specific CompositionRoot.cs.
 */
namespace FOMServer.Shared.Core.FOMPacket
{
    /// <summary>
    /// The main structure for all packets.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Packet
    {
        public PacketIdentifier ID;
        public NetworkAddress Sender;
        public FOMDataUnion Data;
    }
}
