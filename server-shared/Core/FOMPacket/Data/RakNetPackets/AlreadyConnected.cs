using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data.RakNetPackets
{
    [PacketID(PacketIdentifier.ID_ALREADY_CONNECTED)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AlreadyConnected { }
}
