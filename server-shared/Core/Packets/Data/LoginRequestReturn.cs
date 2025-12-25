using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    public enum LoginRequestReturnStatus : byte
    {
        OK = 0,
        VersionMismatch = 1,
        Banned = 2,
    }

    [PacketID(PacketIdentifier.ID_LOGIN_REQUEST_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginRequestReturn
    {
        public const int UsernameSize = 32;

        public LoginRequestReturnStatus Status;
        public fixed byte RawUsername[UsernameSize];

        public string Username
        {
            get
            {
                fixed (byte* ptr = RawUsername)
                    return CStringParser.ToString(ptr, UsernameSize);
            }
        }
    }
}
