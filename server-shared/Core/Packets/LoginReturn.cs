using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(PacketIdentifier.ID_LOGIN_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginReturn
    {
        public const int BanLengthSize = 16;
        public const int BanReasonSize = 129;
        public const int ProcessBlacklistSize = 128;
        public const int FactionMOTDSize = 1024;

        public StatusCode Status;
        public uint PlayerID;

        // ====== PlayerID != 0 ======
        public AccountType AccountType;
        public byte IsVolunteer;
        public byte IsNewPlayer;
        public ushort ClientVersion;

        public byte IsBanned;
        public fixed byte RawBanLength[BanLengthSize]; // IsBanned == 1
        public fixed byte RawBanReason[BanReasonSize]; // IsBanned == 1

        public byte ProcessBlacklistCount;
        public fixed uint ProcessBlacklist[ProcessBlacklistSize];

        public fixed byte RawFactionMOTD[FactionMOTDSize];

        public Apartment DefaultApartment;
        public WorldID DefaultApartmentWorldID;
        public WorldID LoginWorldID;
        // ===========================

        public enum StatusCode : byte
        {
            InvalidLogin = 0, // LOGIN_RETURN_INVALID_LOGIN
            Success = 1, // LOGIN_RETURN_SUCCESS
            UnknownUsername = 2, // LOGIN_RETURN_UNKNOWN_USERNAME
            Unknown3 = 3, // LOGIN_RETURN_3
            IncorrectPassword = 4, // LOGIN_RETURN_INCORRECT_PASSWORD
            CreateCharacter = 5, // LOGIN_RETURN_CREATE_CHARACTER
            CreateCharacterError = 6, // LOGIN_RETURN_CREATE_CHARACTER_ERROR
            TempBanned = 7, // LOGIN_RETURN_TEMP_BANNED
            PermBanned = 8, // LOGIN_RETURN_PERM_BANNED
            DuplicateIP = 9, // LOGIN_RETURN_DUPLICATE_IP
            IntegrityCheckFailed = 10, // LOGIN_RETURN_INTEGRITY_CHECK_FAILED
            RunAsAdmin = 11, // LOGIN_RETURN_RUN_AS_ADMIN
            AccountLocked = 12, // LOGIN_RETURN_ACCOUNT_LOCKED
            NotPurchased = 13, // LOGIN_RETURN_NOT_PURCHASED
        }

        public string BanLength
        {
            set
            {
                fixed (byte* ptr = RawBanLength)
                    CStringParser.FromString(value, ptr, BanLengthSize);
            }
        }

        public string BanReason
        {
            set
            {
                fixed (byte* ptr = RawBanReason)
                    CStringParser.FromString(value, ptr, BanReasonSize);
            }
        }

        public string FactionMOTD
        {
            set
            {
                fixed (byte* ptr = RawFactionMOTD)
                    CStringParser.FromString(value, ptr, FactionMOTDSize);
            }
        }
    }
}
