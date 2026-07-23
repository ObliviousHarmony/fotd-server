using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_PLAYER_LEAVING_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerLeavingWorldPacket
    {
        public uint PlayerId;
    }
}
