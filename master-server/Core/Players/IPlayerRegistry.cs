namespace FOMServer.Master.Core.Players
{
    internal interface IPlayerRegistry
    {
        Player? Get(uint playerID);

        Player Login(ClientSession session);

        void Logout(Player player);
    }
}
