using System.Security.Cryptography;
using System.Text;

namespace FOMServer.Shared.Core.Utilities
{
    /// <summary>
    /// The client never sends a password. On ID_LOGIN it sends
    /// <c>MD5(MD5(password) + username)</c>, each digest rendered as uppercase hex, where the
    /// username is the one it typed (and the one the server echoed back in
    /// ID_LOGIN_REQUEST_RETURN). Accounts therefore store the inner digest, and the server
    /// recomputes the outer one to compare.
    /// </summary>
    /// <remarks>
    /// Read from fom_client.exe 1.8.5.3: HandlePacket_ID_LOGIN_REQUEST_RETURN (0x0049CA70)
    /// hashes the password, appends the echoed username, hashes again, and copies the result
    /// into Packet_ID_LOGIN.passwordHash. The helpers at 0x00565200..0x00565550 are MD5
    /// (four-word init state, 16-byte digest, D76AA478 first round constant) formatted through
    /// a "0123456789ABCDEF" table. MD5 is weak, but it is the client's protocol, not a choice.
    /// </remarks>
    public static class PasswordHashing
    {
        /// <summary>The value to store in <c>account.password</c> for a plaintext password.</summary>
        public static string HashPassword(string password)
        {
            return Md5Hex(password);
        }

        /// <summary>Whether <paramref name="clientHash"/> from ID_LOGIN matches the stored hash.</summary>
        public static bool Verify(string storedHash, string username, string clientHash)
        {
            // An account without a stored hash must never authenticate by accident.
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(clientHash))
            {
                return false;
            }

            var expected = Encoding.ASCII.GetBytes(Md5Hex(storedHash.ToUpperInvariant() + username));
            var actual = Encoding.ASCII.GetBytes(clientHash.ToUpperInvariant());
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        private static string Md5Hex(string input)
        {
            // The client hashes its raw single-byte char buffer, so map characters to bytes 1:1.
            return Convert.ToHexString(MD5.HashData(Encoding.Latin1.GetBytes(input)));
        }
    }
}
