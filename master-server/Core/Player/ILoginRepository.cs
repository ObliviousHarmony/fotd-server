namespace FOMServer.Master.Core.Player
{
    public interface ILoginRepository
    {
        uint? TryLogin(string username, string password);
        bool Logout(uint id);
        void LogoutAllPlayers();
    }
}
