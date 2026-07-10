using FOMServer.Shared.Core.Items;

namespace FOMServer.World.Core.Players
{
    internal interface IItemPacketDispatcher
    {
        void Register(IItemLocation location);

        void Unregister(IItemLocation location);
    }
}
