using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets.RakNet
{
    [PacketId(PacketIdentifier.ID_DISCONNECTION_NOTIFICATION)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DisconnectionNotificationPacket { }
}
