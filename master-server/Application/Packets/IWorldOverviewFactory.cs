using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Packets.Models;

namespace FOMServer.Master.Application.Packets
{
    public interface IWorldOverviewFactory
    {
        WorldOverviewModel Create(Player player);
    }
}
