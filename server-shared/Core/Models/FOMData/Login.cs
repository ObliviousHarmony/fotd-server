using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe public struct Login
    {
        public fixed byte Username[19];
        public fixed byte PasswordHash[32];
        public uint ClientCRC;
        public uint CShellCRC;
        public uint ObjectCRC;
        public fixed byte MACAddress[18];
    }
}
