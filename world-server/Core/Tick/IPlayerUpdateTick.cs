using FOMServer.World.Core.Players;

namespace FOMServer.World.Core.Tick
{
    internal interface IPlayerUpdateTick
    {
        void Register(Player player);

        void Unregister(Player player);

        void QueueUpdate(Player player);
    }
}
