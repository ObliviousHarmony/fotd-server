using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_LOGIN_REQUEST)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginRequest
    {
        public fixed byte RawUsername[BufferSizes.Username];
        public ushort ClientVersion;

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
    }
}
