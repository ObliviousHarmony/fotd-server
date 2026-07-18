using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums.Item;

namespace FOMServer.Shared.Core.Packets.Types.World
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldObject
    {
        public uint Id;
        public ItemType ItemType;
        public byte State;
        public PositionRotation Position;
        public uint OwningPlayerId;
    }
}
