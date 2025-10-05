using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;

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
        public PacketIdentifier* PacketIdentifiers;
    }

    /// <summary>
    /// Represents a network packet to be sent, including its destination, data, and transmission settings.
    /// </summary>
    /// <remarks>
    /// This structure encapsulates all the necessary information for sending a packet over the network. It
    /// includes the destination address, the packet data, and various transmission options such as priority, reliability,
    /// and ordering channel.
    ///
    /// Must match the struct in `fom-network/include/fom-network/PacketAPI.h`
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SendPacket
    {
        public PacketIdentifier ID;
        public FOMDataUnion Data;
        public NetworkAddress NetworkAddress;
        public PacketPriority Priority;
        public PacketReliability Reliability;
        public byte OrderingChannel;
        public byte Broadcast;
    }

}
