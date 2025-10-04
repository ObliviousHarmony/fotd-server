using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data.RakNetPackets
{
    [PacketID(PacketIdentifier.ID_INVALID_PASSWORD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InvalidPassword { }
}
