using FOMServer.Shared.Core.Enums;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Models
{
    /// <summary>
    /// Represents a world on the world overview map.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OverviewWorld
    {
        /// <summary>
        /// A buffer for passing a variable number of OverviewWorld structs across the interop boundary.
        /// </summary>
        [InlineArray((int)WorldID.NUM_WORLDS)]
        public struct Buffer
        {
            public OverviewWorld OverviewWorld;
        }

        public WorldID ID;
        public NetworkAddress Address;
        public ushort PlayerCount;
        public Faction ControllingFaction;
        public FactionRelation ControllingFactionRelation;
    }
}
