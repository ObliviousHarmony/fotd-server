using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data.RakNetPackets
{
    [PacketID(PacketIdentifier.ID_CONNECTION_LOST)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ConnectionLost { }
}
