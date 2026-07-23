using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_WORLD_LOGIN_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldLoginReturnPacket
    {
        public StatusCode Status;
        public WorldId WorldId;
        public NetworkAddress WorldServerAddress;

        public enum StatusCode : byte
        {
            Invalid = 0, // WORLD_LOGIN_RETURN_INVALID
            Success = 1, // WORLD_LOGIN_RETURN_SUCCESS
            ServerOffline = 2, // WORLD_LOGIN_RETURN_SERVER_OFFLINE
            WrongFaction = 3, // WORLD_LOGIN_RETURN_WRONG_FACTION
            WorldFull = 4, // WORLD_LOGIN_RETURN_WORLD_FULL
            UnknownError = 5, // WORLD_LOGIN_RETURN_UNKNOWN_ERROR
            NoFactionPrivileges = 6, // WORLD_LOGIN_RETURN_NO_FACTION_PRIVILEGES
            OutOfRange = 7, // WORLD_LOGIN_RETURN_OUT_OF_RANGE
            Retry = 8, // WORLD_LOGIN_RETURN_RETRY
        }
    }
}
