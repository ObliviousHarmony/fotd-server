using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_CHECK_NAME)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CheckName
    {
        public const int NameSize = 20;

        public fixed byte RawName[NameSize];

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                    return CStringParser.ToString(ptr, NameSize);
            }
        }
    }
}
