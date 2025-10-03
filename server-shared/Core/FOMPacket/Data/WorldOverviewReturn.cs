using System.Runtime.InteropServices;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldOverviewReturn
    {
        public uint PlayerID;
        public WorldOverviewModel WorldOverview;
    }
}
