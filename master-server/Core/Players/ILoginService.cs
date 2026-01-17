using FOMServer.Shared.Core.Packets.Types;

namespace FOMServer.Master.Core.Players
{
    public interface ILoginService
    {
        Player? Login(string username, string password, NetworkAddress clientAddress);
        bool Logout(Player player);
    }
}
