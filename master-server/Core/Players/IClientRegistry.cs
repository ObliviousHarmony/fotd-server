using FOMServer.Shared.Interop.FOMNetwork;

namespace FOMServer.Master.Core.Players
{
    internal interface IClientRegistry
    {
        ClientSession? Get(NetworkAddress address);

        ClientSession? Get(uint playerId);

        ClientSession Register(NetworkAddress address);

        void BeginLogin(ClientSession session, uint playerId);

        bool Unregister(ClientSession session);
    }
}
