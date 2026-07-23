using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Faction
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionEmblemInterop
    {
        public uint StaticEmblemId;
        public LayersArray Layers;

        [InlineArray(FactionConstants.NumFactionEmblemLayers)]
        public struct LayersArray
        {
            private FactionEmblemLayerInterop _element;
        }
    }
}
