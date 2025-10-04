using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_LOGIN_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LoginReturn
    {
        public enum StatusCode : byte
        {
            LOGIN_RETURN_INVALID_LOGIN = 0,
            LOGIN_RETURN_SUCCESS = 1,
            LOGIN_RETURN_INVALID_USERNAME = 2,
            LOGIN_RETURN_X1 = 3, // Unknown
            LOGIN_RETURN_INVALID_PASSWORD = 4,
            LOGIN_RETURN_CREATE_CHARACTER = 5,
            LOGIN_RETURN_CREATE_CHARACTER_ERROR = 6,
            LOGIN_RETURN_TEMP_BANNED = 7,
            LOGIN_RETURN_PERM_BANNED = 8,
            LOGIN_RETURN_DUPLICATE_ACCOUNTS = 9,
            LOGIN_RETURN_INTEGRITY_CHECK_FAILED = 10,
            LOGIN_RETURN_CLIENT_ERROR = 11,
            LOGIN_RETURN_LOCKED = 12
        }

        public StatusCode Status;
        public uint PlayerID;
        public byte AccountType;
        public byte RawIsVolunteer;
        public ushort ClientVersion;
        public WorldOverviewModel WorldOverview;

        public bool IsVolunteer
        {
            get => RawIsVolunteer != 0;
            set => RawIsVolunteer = (byte)(value ? 1 : 0);
        }
    }
}
