using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_WORLD_LOGIN_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldLoginReturn
    {
        public enum StatusCode : byte
        {
            WORLD_LOGIN_RETURN_INVALID = 0,
            WORLD_LOGIN_RETURN_SUCCESS = 1,
            WORLD_LOGIN_RETURN_SERVER_UNAVAILABLE = 2,
            WORLD_LOGIN_RETURN_FACTION_INACCESSIBLE = 3,
            WORLD_LOGIN_RETURN_SERVER_FULL = 4,
            WORLD_LOGIN_RETURN_FACTION_REVOKED = 5,
        }

        public StatusCode Status;
        public WorldID WorldID;
    }
}
