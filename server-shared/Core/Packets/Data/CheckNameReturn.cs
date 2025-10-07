using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets.Data
{
    [PacketID(PacketIdentifier.ID_CHECK_NAME_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CheckNameReturn
    {
        public uint ExistingPlayerID;
    }
}
