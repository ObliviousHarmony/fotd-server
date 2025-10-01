using FOMServer.Shared.Core.Enums;
using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models
{
    /// <summary>
    /// Represents a world on the world overview map.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OverviewWorld
    {
        public WorldID ID;
        public NetworkAddress Address;
        public ushort PlayerCount;
        public Faction ControllingFaction;
        public FactionRelation ControllingFactionRelation;
    }
}
