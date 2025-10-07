using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_WORLD_OVERVIEW)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldOverview
    {
        public uint PlayerID;
    }
}
