using System.Runtime.InteropServices;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.Item
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemInterop
    {
        public uint Id;
        public ItemBaseInterop Base;
    }
}
