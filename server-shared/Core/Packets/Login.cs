using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_LOGIN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Login
    {
        public const int PasswordHashSize = 64;
        public const int MacAddressSize = 32;
        public const int DriveCount = 4;
        public const int DriveModelSize = 64;
        public const int DriveSerialNumberSize = 32;
        public const int LoginTokenSize = 64;
        public const int ComputerNameSize = 32;
        public const int SteamTicketSize = 1024;

        public fixed byte RawUsername[BufferSizes.Username];
        public fixed byte RawPasswordHash[PasswordHashSize];
        public uint ClientCrc;
        public uint CShellCrc;
        public uint ObjectCrc;
        public fixed byte RawMacAddress[MacAddressSize];
        public fixed byte RawDriveModels[DriveCount * DriveModelSize];
        public fixed byte RawDriveSerialNumbers[DriveCount * DriveSerialNumberSize];
        public fixed byte RawLoginToken[LoginTokenSize];
        public fixed byte RawComputerName[ComputerNameSize];

        public byte HasSteamTicket;
        public int SteamTicketLength; // HasSteamTicket == 1
        public fixed byte RawSteamTicket[SteamTicketSize]; // HasSteamTicket == 1

        public string Username
        {
            get
            {
                fixed (byte* ptr = RawUsername)
                {
                    return CStringParser.ToString(ptr, BufferSizes.Username);
                }
            }
        }

        public string PasswordHash
        {
            get
            {
                fixed (byte* ptr = RawPasswordHash)
                {
                    return CStringParser.ToString(ptr, PasswordHashSize);
                }
            }
        }

        public string MacAddress
        {
            get
            {
                fixed (byte* ptr = RawMacAddress)
                {
                    return CStringParser.ToString(ptr, MacAddressSize);
                }
            }
        }

        public string ComputerName
        {
            get
            {
                fixed (byte* ptr = RawComputerName)
                {
                    return CStringParser.ToString(ptr, ComputerNameSize);
                }
            }
        }

        public string GetDriveModel(int index)
        {
            if (index is < 0 or >= DriveCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            fixed (byte* ptr = RawDriveModels)
            {
                return CStringParser.ToString(ptr + (index * DriveModelSize), DriveModelSize);
            }
        }

        public string GetDriveSerialNumber(int index)
        {
            if (index is < 0 or >= DriveCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            fixed (byte* ptr = RawDriveSerialNumbers)
            {
                return CStringParser.ToString(ptr + (index * DriveSerialNumberSize), DriveSerialNumberSize);
            }
        }
    }
}
