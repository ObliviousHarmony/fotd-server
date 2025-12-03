using FOMServer.Shared.Core.Packets;

namespace FOMServer.World.Core.Players
{
    public interface IWorldLoginService
    {
        void Prepare(uint playerID, byte selectedNodeID);
        WorldLoginResult? Login(uint playerID, NetworkAddress clientAddress);
    }
}
