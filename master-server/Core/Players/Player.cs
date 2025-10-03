using FOMServer.Shared.Core.FOMPacket;

namespace FOMServer.Master.Core.Players
{
    public class Player
    {
        public NetworkAddress ClientAddress { get; init; }
        public uint ID { get; init; }
        public string Username { get; init; } = "";
        public bool HasCharacter { get; set; }
    }
}
