using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe public struct LoginRequest
    {
        public fixed byte Username[19];
        public ushort ClientVersion;
    }
}
