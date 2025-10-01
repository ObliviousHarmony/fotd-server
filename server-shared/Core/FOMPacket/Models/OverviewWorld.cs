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
        WorldID ID;
        NetworkAddress Address;
        ushort PlayerCount;
        Faction ControllingFaction;
        FactionRelation ControllingFactionRelation;
    }
}
