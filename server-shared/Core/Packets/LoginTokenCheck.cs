using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(PacketIdentifier.ID_LOGIN_TOKEN_CHECK)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginTokenCheck
    {
        public const int RequestTokenSize = 32;

        public byte FromServer;

        public fixed byte RawRequestToken[RequestTokenSize];  // FromServer == 0

        public byte Success;                                  // FromServer == 1
        public fixed byte RawUsername[BufferSizes.Username];          // FromServer == 1

        public string RequestToken
        {
            get
            {
                fixed (byte* ptr = RawRequestToken)
                    return CStringParser.ToString(ptr, RequestTokenSize);
            }
        }

        public string Username
        {
            get
            {
                fixed (byte* ptr = RawUsername)
                    return CStringParser.ToString(ptr, BufferSizes.Username);
            }
        }
    }
}
