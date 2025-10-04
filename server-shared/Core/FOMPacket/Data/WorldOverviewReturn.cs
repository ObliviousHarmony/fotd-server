using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_WORLD_OVERVIEW_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldOverviewReturn
    {
        public uint PlayerID;
        public WorldOverviewModel WorldOverview;
    }
}
