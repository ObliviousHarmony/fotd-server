using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(PacketIdentifier.ID_PLAYER_WORLD_READY)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerWorldReady
    {
        public uint PlayerID;
        public StatusCode Status;

        public enum StatusCode : byte
        {
            Success = 0,
            PlayerNotFound = 1,
        }
    }
}
