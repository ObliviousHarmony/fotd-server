using System.Runtime.InteropServices;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Structs;

namespace FOMServer.Shared.Interop.FOMNetwork.Structs.World
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldObjectInterop
    {
        public uint Id;
        public ItemType ItemType;
        public byte State;
        public PositionRotationInterop Position;
        public uint OwningPlayerId;
    }
}
