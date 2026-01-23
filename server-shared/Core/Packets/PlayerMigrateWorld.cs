using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(PacketIdentifier.ID_PLAYER_MIGRATE_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerMigrateWorld
    {
        public uint PlayerID;
    }
}
