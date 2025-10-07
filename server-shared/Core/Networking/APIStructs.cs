using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    /// <summary>
    /// An entry for describing the size of the managed struct for a packet identifier.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketStructure
    {
        // Must match the struct in `fom-network/include/fom-network/NetworkAPI.h`
        public PacketIdentifier ID;
        public int Size;
    }

    /// <summary>
    /// Represents a collection of received network packets.
    /// </summary>
    /// <remarks>
    /// This structure is used to hold a pointer to an array of received packets and the count of packets
    /// in the array. The memory layout of this structure is sequential and tightly packed to ensure compatibility with
    /// unmanaged code.
    ///
    /// Must match the struct in `fom-network/include/fom-network/PacketAPI.h`
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ReceivedPackets
    {
        public byte Count;
        public IntPtr Packets;
        public NetworkAddress* Senders;
        public PacketIdentifier* Identifiers;
    }

    /// <summary>
    /// Represents a network packet to be sent, including its destination, data, and transmission settings.
    /// </summary>
    /// <remarks>
    /// This structure contains a pointer to the packet's data in managed memory. This is done so that
    /// the packet data can be read from a shared buffer to avoid unnecessary pinning of small memory
    /// blocks.
    ///
    /// Must match the struct in `fom-network/include/fom-network/PacketAPI.h`
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SendPacket
    {
        public PacketIdentifier ID;
        public IntPtr Data;
        public NetworkAddress NetworkAddress;
        public PacketPriority Priority;
        public PacketReliability Reliability;
        public byte OrderingChannel;
        public byte Broadcast;
    }

}
