using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_PLAYER_ENTERING_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerEnteringWorld
    {
        public uint PlayerID;
        public byte SelectedNodeID;
    }
}
