using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.World.Core.Player
{
    public interface ILoginService
    {
        void Prepare(uint playerID, byte selectedNodeID);
        LoginContext? Login(uint playerID, NetworkAddress clientAddress);
    }
}
