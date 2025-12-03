using FOMServer.Shared.Core.Packets;

namespace FOMServer.World.Core.Players
{
    public interface ILoginService
    {
        void Prepare(uint playerID, byte selectedNodeID);
        LoginContext? Login(uint playerID, NetworkAddress clientAddress);
    }
}
