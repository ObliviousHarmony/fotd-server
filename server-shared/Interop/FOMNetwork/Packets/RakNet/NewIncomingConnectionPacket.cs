using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets.RakNet
{
    [PacketId(PacketIdentifier.ID_NEW_INCOMING_CONNECTION)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NewIncomingConnectionPacket { }
}
