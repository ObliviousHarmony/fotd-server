using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_LOGIN_REQUEST_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginRequestReturn
    {
        public enum StatusCode : byte
        {
            LOGIN_REQUEST_INVALID_INFORMATION = 0,
            LOGIN_REQUEST_SUCCESS = 1,
            LOGIN_REQUEST_OUTDATED_CLIENT = 2,
            LOGIN_REQUEST_ALREADY_LOGGED_IN = 3
        }

        public StatusCode Status;
        public fixed byte RawUsername[19];
    }
}
