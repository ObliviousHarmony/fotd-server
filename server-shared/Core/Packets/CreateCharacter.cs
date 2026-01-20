using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(PacketIdentifier.ID_CREATE_CHARACTER)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CreateCharacter
    {
        public const int NameSize = 20;
        public const int BiographySize = 511;

        public uint PlayerID;
        public Avatar Avatar;
        public fixed byte RawName[NameSize];
        public fixed byte RawBiography[BiographySize];

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                    return CStringParser.ToString(ptr, NameSize);
            }
        }

        public string Biography
        {
            get
            {
                fixed (byte* ptr = RawBiography)
                    return CStringParser.ToString(ptr, BiographySize);
            }
        }
    }
}
