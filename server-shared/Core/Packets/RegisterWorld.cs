using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketID(PacketIdentifier.ID_REGISTER_WORLD)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegisterWorld
    {
        public NetworkAddress ClientAddress;
        public byte WorldIDCount;
        public WorldIDArray WorldIDs;

        [InlineArray((int)WorldID.NUM_WORLDS)]
        public struct WorldIDArray
        {
            private WorldID _worldID;
        }
    }
}
