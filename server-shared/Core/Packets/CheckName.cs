using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_CHECK_NAME)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CheckName
    {
        public fixed byte RawName[BufferSizes.PlayerName];
        public uint PlayerId;

        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                {
                    return CStringParser.ToString(ptr, BufferSizes.PlayerName);
                }
            }
        }
    }
}
