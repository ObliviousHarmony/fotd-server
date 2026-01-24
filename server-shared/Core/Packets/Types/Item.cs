using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Item
    {
        public uint ID;
        public ItemBase Base;
    }
}
