using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_PLAYER_ENTERING_WORLD_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerEnteringWorldReturn
    {
        public enum StatusCode : byte
        {
            PLAYER_ENTERING_WORLD_RETURN_ERROR = 0,
            PLAYER_ENTERING_WORLD_RETURN_ALREADY_IN_WORLD = 1,
            PLAYER_ENTERING_WORLD_RETURN_READY = 2,
            PLAYER_ENTERING_WORLD_RETURN_SERVER_FULL = 3,
        }

        public StatusCode Status;
        public uint PlayerID;
    }
}
