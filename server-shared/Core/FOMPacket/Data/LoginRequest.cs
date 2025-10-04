using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_LOGIN_REQUEST)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginRequest
    {
        public fixed byte RawUsername[19];
        public ushort ClientVersion;

        public string Username
        {
            get
            {
                fixed (byte* ptr = RawUsername)
                    return CStringParser.ToString(ptr, 19);
            }
        }
    }
}
