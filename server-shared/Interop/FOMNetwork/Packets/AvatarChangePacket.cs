using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Structs;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_AVATAR_CHANGE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AvatarChangePacket
    {
        public uint PlayerId;
        public AvatarInterop Avatar;
    }
}
