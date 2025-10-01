using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
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
