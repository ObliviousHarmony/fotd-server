namespace FOMServer.Master.Core.Players
{
    public interface ILoginRepository
    {
        uint? TryLogin(string username, string password);
        bool Logout(uint id);
        void LogoutAllPlayers();
    }
}
