namespace FOMServer.Master.Core.Players
{
    internal interface IPlayerRegistry
    {
        Player? Get(uint playerId);

        Player Login(ClientSession session);

        void Logout(Player player);
    }
}
