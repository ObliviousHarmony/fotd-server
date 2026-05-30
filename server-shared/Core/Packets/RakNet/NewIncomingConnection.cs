using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.RakNet
{
    [PacketId(PacketIdentifier.ID_NEW_INCOMING_CONNECTION)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NewIncomingConnection { }
}
