using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
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
