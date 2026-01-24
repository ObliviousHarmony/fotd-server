using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FactionPerk
    {
        public ushort ID;
        public byte Level;
        public byte Active;
    }
}
