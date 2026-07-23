using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_REGISTER_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterWorldPacket
    {
        public NetworkAddress PublicAddress;
        public byte WorldIdCount;
        public WorldIdArray WorldIds;

        [InlineArray((int)WorldId.NUM_WORLDS)]
        public struct WorldIdArray
        {
            private WorldId _worldId;
        }
    }
}
