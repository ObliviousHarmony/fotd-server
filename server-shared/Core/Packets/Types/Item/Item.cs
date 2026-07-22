using System.Runtime.InteropServices;

namespace FOMServer.Shared.Core.Packets.Types.Item
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Item
    {
        public uint Id;
        public ItemBase Base;
    }
}
