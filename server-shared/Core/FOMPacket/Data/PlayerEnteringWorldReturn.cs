using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerEnteringWorldReturn
    {
        public enum StatusCode : byte
        {
            PLAYER_ENTERING_WORLD_RETURN_ERROR = 0,
            PLAYER_ENTERING_WORLD_RETURN_READY = 1,
            PLAYER_ENTERING_WORLD_RETURN_SERVER_FULL = 2,
        }

        public StatusCode Status;
        public uint PlayerID;
    }
}
