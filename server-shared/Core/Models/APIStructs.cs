using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Models.FOMData;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models
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
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReceivedPackets
    {
        // Must match the struct in `fom-network/include/fom-network/PacketAPI.h`
        public IntPtr Packets;
        public int Count;
    }

    /// <summary>
    /// Represents a network packet to be sent, including its destination, data, and transmission settings.
    /// </summary>
    /// <remarks>
    /// This structure encapsulates all the necessary information for sending a packet over the network. It
    /// includes the destination address, the packet data, and various transmission options such as priority, reliability,
    /// and ordering channel.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SendPacket
    {
        // Must match the struct in `fom-network/include/fom-network/PacketAPI.h`
        public PacketIdentifier ID;
        public FOMDataUnion Data;
        public NetworkAddress NetworkAddress;
        public PacketPriority Priority;
        public PacketReliability Reliability;
        public byte OrderingChannel;
        public byte Broadcast;
    }

}
