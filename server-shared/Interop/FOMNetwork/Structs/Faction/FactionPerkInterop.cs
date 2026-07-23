using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Faction
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionPerkInterop
    {
        public ushort Id;
        public byte Level;
        public byte Active;
    }
}
