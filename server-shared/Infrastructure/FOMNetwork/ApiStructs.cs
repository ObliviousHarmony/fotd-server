using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using NetworkAddress = FOMServer.Shared.Core.Packets.Types.NetworkAddress;

namespace FOMServer.Shared.Infrastructure.FOMNetwork
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketStructure
    {
        // Must match the struct in `fom-network/include/fom-network/NetworkApi.h`
        public PacketIdentifier Id;
        public int Size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ReceivedPackets
    {
        // Must match the struct in `fom-network/include/fom-network/PacketApi.h`
        public byte Count;
        public IntPtr Packets;
        public NetworkAddress* Senders;
        public PacketIdentifier* Identifiers;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SendPacket
    {
        // Must match the struct in `fom-network/include/fom-network/PacketApi.h`
        public PacketIdentifier Id;
        public IntPtr Data;
        public int NumNetworkAddresses;
        public IntPtr NetworkAddresses;
        public PacketPriority Priority;
        public PacketReliability Reliability;
        public byte OrderingChannel;
        public byte Broadcast;
    }
}
