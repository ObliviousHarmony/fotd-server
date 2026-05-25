using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.World.Core.Players
{
    internal class ClientSession
    {
        private readonly Lock _syncRoot = new();

        public ClientSession(NetworkAddress address, uint playerID)
        {
            Address = address;
            PlayerID = playerID;
        }

        public NetworkAddress Address { get; }

        public uint PlayerID { get; }
    }
}
