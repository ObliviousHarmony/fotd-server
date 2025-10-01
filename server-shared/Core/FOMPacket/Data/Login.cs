using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Login
    {
        public fixed byte RawUsername[19];
        public fixed byte RawPasswordHash[32];
        public uint ClientCRC;
        public uint CShellCRC;
        public uint ObjectCRC;
        public fixed byte RawMACAddress[18];

        public string Username
        {
            get
            {
                fixed (byte* ptr = RawUsername)
                    return CStringParser.ToString(ptr, 19);
            }
        }

        public string PasswordHash
        {
            get
            {
                fixed (byte* ptr = RawPasswordHash)
                    return CStringParser.ToString(ptr, 32);
            }
        }

        public string MACAddress
        {
            get
            {
                fixed (byte* ptr = RawMACAddress)
                    return CStringParser.ToString(ptr, 18);
            }
        }
    }
}
