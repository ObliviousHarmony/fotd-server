using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data.RakNetPackets
{
    [PacketID(PacketIdentifier.ID_CONNECTION_REQUEST_ACCEPTED)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ConnectionRequestAccepted { }
}
