using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.World.Core.Players
{
    internal interface IClientRegistry
    {
        ClientSession? Get(NetworkAddress address);
        ClientSession? Get(uint playerID);

        ClientSession Register(NetworkAddress address);
        void BeginLogin(ClientSession session, uint playerID);
        bool Unregister(ClientSession session);
    }
}
