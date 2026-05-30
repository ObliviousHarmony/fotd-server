using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.RakNet
{
    [PacketId(PacketIdentifier.ID_DISCONNECTION_NOTIFICATION)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DisconnectionNotification { }
}
