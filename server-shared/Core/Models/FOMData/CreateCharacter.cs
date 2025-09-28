using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Models.FOMData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CreateCharacter
    {
        public uint AccountID;
        public Avatar Avatar;
        public fixed byte RawName[20];
        public fixed byte RawBiography[511];

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                    return CStringParser.ToString(ptr, 20);
            }
        }

        public string Biography
        {
            get
            {
                fixed (byte* ptr = RawBiography)
                    return CStringParser.ToString(ptr, 511);
            }
        }
    }
}
