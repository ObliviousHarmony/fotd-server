using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Interop.FOMNetwork.Packets
{
    [PacketId(PacketIdentifier.ID_WORLD_UPDATE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldUpdatePacket
    {
        public const int MaxWorldUpdates = 100; // MAX_WORLD_UPDATES

        public uint PlayerId;
        public uint Unknown1;
        public byte UpdateCount;
        public UpdatesArray Updates;

        [InlineArray(MaxWorldUpdates)]
        public struct UpdatesArray
        {
            private Structs.WorldUpdateInterop _element;
        }
    }
}
