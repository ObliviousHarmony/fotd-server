using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_CHECK_NAME)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CheckNamePacket
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
