using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.World.Core.Players
{
    public class Player
    {
        public NetworkAddress ClientAddress { get; init; }
        public uint ID { get; init; }
        public byte SelectedNodeID { get; init; }
    }
}
