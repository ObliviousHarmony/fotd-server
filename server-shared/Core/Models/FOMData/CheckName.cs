using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe public struct CheckName
    {
        public fixed byte Name[20];
    }
}
