using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets;

namespace FOMServer.Shared.Core.Networking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketStructure
    {
        // Must match the struct in `fom-network/include/fom-network/NetworkAPI.h`
        public PacketIdentifier ID;
        public int Size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ReceivedPackets
    {
        // Must match the struct in `fom-network/include/fom-network/PacketAPI.h`
        public byte Count;
        public IntPtr Packets;
        public NetworkAddress* Senders;
        public PacketIdentifier* Identifiers;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SendPacket
    {
        // Must match the struct in `fom-network/include/fom-network/PacketAPI.h`
        public PacketIdentifier ID;
        public IntPtr Data;
        public int NumNetworkAddresses;
        public IntPtr NetworkAddresses;
        public PacketPriority Priority;
        public PacketReliability Reliability;
        public byte OrderingChannel;
        public byte Broadcast;
    }

}
