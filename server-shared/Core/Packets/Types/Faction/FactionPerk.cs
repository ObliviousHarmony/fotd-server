using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types.Faction
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionPerk
    {
        public ushort Id;
        public byte Level;
        public byte Active;
    }
}
