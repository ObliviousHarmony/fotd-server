using FOMServer.Shared.Core.Utilities;

namespace FOMServer.Shared.Tests
{
    public class PasswordHashingTests
    {
        // Expected values computed with openssl, not with the code under test.
        private const string StoredHash = "5F4DCC3B5AA765D61D8327DEB882CF99"; // MD5("password")
        private const string ClientHash = "2DD901A5A846AF3A230E0548738BD19C"; // MD5(StoredHash + "TestPlayer")

        [Fact]
        public void HashPassword_IsUppercaseMd5Hex()
        {
            Assert.Equal(StoredHash, PasswordHashing.HashPassword("password"));
        }

        [Fact]
        public void Verify_AcceptsTheClientsDoubleHash()
        {
            Assert.True(PasswordHashing.Verify(StoredHash, "TestPlayer", ClientHash));
        }

        [Fact]
        public void Verify_IgnoresHexCase()
        {
            Assert.True(
                PasswordHashing.Verify(StoredHash.ToLowerInvariant(), "TestPlayer", ClientHash.ToLowerInvariant())
            );
        }

        [Fact]
        public void Verify_SaltsWithTheUsername()
        {
            Assert.False(PasswordHashing.Verify(StoredHash, "OtherPlayer", ClientHash));
        }

        [Fact]
        public void Verify_RejectsTheWrongPassword()
        {
            var wrong = PasswordHashing.HashPassword("hunter2");
            Assert.False(PasswordHashing.Verify(wrong, "TestPlayer", ClientHash));
        }

        [Fact]
        public void Verify_RejectsAnAccountWithNoStoredHash()
        {
            Assert.False(PasswordHashing.Verify("", "TestPlayer", ClientHash));
            Assert.False(PasswordHashing.Verify(StoredHash, "TestPlayer", ""));
        }
    }
}
