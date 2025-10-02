using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LoginReturn
    {
        [InlineArray((int)WorldID.NUM_WORLDS)]
        public struct OverviewWorldArray
        {
            public OverviewWorld OverviewWorld;
        }

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
        public ushort ClientVersion;
        public byte NumWorlds;
        public OverviewWorldArray WorldBuffer;
        public uint OnlinePlayers;
        public byte RawIsPrisoner;

        public bool IsPrisoner
        {
            get => RawIsPrisoner != 0;
            set => RawIsPrisoner = (byte)(value ? 1 : 0);
        }
    }
}
