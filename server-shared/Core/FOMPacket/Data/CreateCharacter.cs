using System.Runtime.InteropServices;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CreateCharacter
    {
        public uint PlayerID;
        public AvatarModel Avatar;
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
