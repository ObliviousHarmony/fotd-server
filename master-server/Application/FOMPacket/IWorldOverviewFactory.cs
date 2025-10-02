using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Master.Application.FOMPacket
{
    public interface IWorldOverviewFactory
    {
        WorldOverview Create(Player player);
    }
}
