using System.Runtime.InteropServices;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LoginReturn
    {
        public enum StatusCode : byte
        {
            LOGIN_RETURN_INVALID_LOGIN,
            LOGIN_RETURN_SUCCESS,
            LOGIN_RETURN_INVALID_USERNAME,
            LOGIN_RETURN_X1, // Unknown
            LOGIN_RETURN_INVALID_PASSWORD,
            LOGIN_RETURN_CREATE_CHARACTER,
            LOGIN_RETURN_CREATE_CHARACTER_ERROR,
            LOGIN_RETURN_TEMP_BANNED,
            LOGIN_RETURN_PERM_BANNED,
            LOGIN_RETURN_DUPLICATE_ACCOUNTS,
            LOGIN_RETURN_INTEGRITY_CHECK_FAILED,
            LOGIN_RETURN_CLIENT_ERROR,
            LOGIN_RETURN_LOCKED
        }

        public StatusCode Status;
        public uint PlayerID;
        public byte AccountType;
        public byte RawIsVolunteer;
        public ushort ClientVersion;
        public byte NumWorlds;
        public OverviewWorld.Buffer WorldBuffer;
        public uint OnlinePlayers;
        public uint OnlineNewPlayers;
        public byte RawIsPrisoner;
        public Apartment DefaultApartment;

        public bool IsVolunteer
        {
            get => RawIsVolunteer != 0;
            set => RawIsVolunteer = (byte)(value ? 1 : 0);
        }

        public bool IsPrisoner
        {
            get => RawIsPrisoner != 0;
            set => RawIsPrisoner = (byte)(value ? 1 : 0);
        }
    }
}
