using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Constants;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Faction
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionPerksInterop
    {
        public uint Unknown1;
        public uint Unknown2;
        public uint Count;
        public PerksArray Perks;

        [InlineArray(FactionConstants.MaxFactionPerks)]
        public struct PerksArray
        {
            private FactionPerkInterop _element;
        }
    }
}
