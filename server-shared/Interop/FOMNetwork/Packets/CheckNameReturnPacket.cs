using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_CHECK_NAME_RETURN)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CheckNameReturnPacket
    {
        public uint OwnerPlayerId;
    }
}
