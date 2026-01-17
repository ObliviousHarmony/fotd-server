using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.RakNet
{
    [PacketID(PacketIdentifier.ID_ALREADY_CONNECTED)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AlreadyConnected { }
}
