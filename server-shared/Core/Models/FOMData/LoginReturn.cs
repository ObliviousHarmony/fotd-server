using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
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
    }
}
