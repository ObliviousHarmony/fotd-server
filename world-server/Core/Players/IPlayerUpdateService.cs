namespace FOMServer.World.Core.Players
{
    internal interface IPlayerUpdateService
    {
        void RegisterRecipient(Player player);

        void UnregisterRecipient(Player player);

        void QueueUpdate(Player player);
    }
}
