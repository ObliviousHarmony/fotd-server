using FOMServer.Shared.Core.Packets;

namespace FOMServer.World.Core.Players
{
    public class Player
    {
        public NetworkAddress ClientAddress { get; set; }
        public uint ID { get; init; }
        public byte SelectedNodeID { get; init; }
    }
}
