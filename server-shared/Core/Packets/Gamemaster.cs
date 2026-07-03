using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    // Every staff/GM command the client issues arrives as this single packet;
    // <see cref="Command"/> selects which one. Only the spawn command's tail
    // (<see cref="Item"/> + <see cref="Quantity"/>) is modelled; for other
    // commands those fields are left zeroed by the native serializer.
    [PacketId(PacketIdentifier.ID_GAMEMASTER)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Gamemaster
    {
        public uint PlayerId;
        public GamemasterCommand Command;
        public Item Item;
        public uint Quantity;
    }
}
