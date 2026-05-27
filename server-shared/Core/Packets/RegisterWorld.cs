using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_REGISTER_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterWorld
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
