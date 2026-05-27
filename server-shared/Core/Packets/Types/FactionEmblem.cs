using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionEmblem
    {
        public uint StaticEmblemId;
        public LayersArray Layers;

        [InlineArray(FactionConstants.NumFactionEmblemLayers)]
        public struct LayersArray
        {
            private FactionEmblemLayer _element;
        }
    }
}
