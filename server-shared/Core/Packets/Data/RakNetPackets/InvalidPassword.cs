using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data.RakNetPackets
{
    [PacketID(PacketIdentifier.ID_INVALID_PASSWORD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InvalidPassword { }
}
