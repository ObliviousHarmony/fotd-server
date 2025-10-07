using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_LOGIN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Login
    {
        public const int UsernameSize = 19;
        public const int PasswordHashSize = 32;
        public const int MACAddressSize = 18;

        public fixed byte RawUsername[UsernameSize];
        public fixed byte RawPasswordHash[PasswordHashSize];
        public uint ClientCRC;
        public uint CShellCRC;
        public uint ObjectCRC;
        public fixed byte RawMACAddress[MACAddressSize];

        public string Username
        {
            get
            {
                fixed (byte* ptr = RawUsername)
                    return CStringParser.ToString(ptr, UsernameSize);
            }
        }

        public string PasswordHash
        {
            get
            {
                fixed (byte* ptr = RawPasswordHash)
                    return CStringParser.ToString(ptr, PasswordHashSize);
            }
        }

        public string MACAddress
        {
            get
            {
                fixed (byte* ptr = RawMACAddress)
                    return CStringParser.ToString(ptr, MACAddressSize);
            }
        }
    }
}
