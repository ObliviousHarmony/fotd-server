using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CheckName
    {
        public fixed byte RawName[20];

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                    return CStringParser.ToString(ptr, 20);
            }
        }
    }
}
