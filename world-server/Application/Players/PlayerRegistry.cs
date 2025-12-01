using FOMServer.Shared.Application.Players;
using FOMServer.Shared.Core.Packets;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Players
{
    public class PlayerRegistry : BasePlayerRegistry<Player>, IPlayerRegistry
    {
        protected override Player Load(uint id, NetworkAddress clientAddress)
        {
            return new Player
            {
                ID = id,
                ClientAddress = clientAddress
            };
        }
    }
}
