using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Metadata;

namespace FOMServer.Shared.Core.FOMPacket.Data
{
    [PacketID(PacketIdentifier.ID_CHECK_NAME)]
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
