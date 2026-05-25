using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(PacketIdentifier.ID_LOGIN_REQUEST_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginRequestReturn
    {
        public StatusCode Status;
        public fixed byte RawUsername[BufferSizes.Username];

        public enum StatusCode : byte
        {
            Invalid = 0, // LOGIN_REQUEST_RETURN_INVALID_INFORMATION
            Success = 1, // LOGIN_REQUEST_RETURN_SUCCESS
            VersionMismatch = 2, // LOGIN_REQUEST_RETURN_VERSION_MISMATCH
            AlreadyLoggedIn = 3, // LOGIN_REQUEST_RETURN_ALREADY_LOGGED_IN
        }

        public string Username
        {
            set
            {
                fixed (byte* ptr = RawUsername)
                {
                    CStringParser.FromString(value, ptr, BufferSizes.Username);
                }
            }
        }
    }
}
