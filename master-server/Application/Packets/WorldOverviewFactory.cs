using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Models;

namespace FOMServer.Master.Application.Packets
{
    public class WorldOverviewFactory : IWorldOverviewFactory
    {
        private readonly IWorldServerService _worldServerService;

        public WorldOverviewFactory(IWorldServerService worldServerService)
        {
            _worldServerService = worldServerService;
        }

        public WorldOverviewModel Create(Player player)
        {
            var worldOverview = new WorldOverviewModel()
            {
                OnlinePlayers = 0,
                OnlineNewPlayers = 0,
                IsPrisoner = false,
            };

            var worldServers = _worldServerService.GetAll();
            worldOverview.NumWorlds = (byte)worldServers.Length;
            for (var i = 0; i < worldServers.Length; ++i)
            {
                var server = worldServers[i];

                worldOverview.WorldBuffer[i].ID = server.ID;
                worldOverview.WorldBuffer[i].Address = server.ClientAddress;
                worldOverview.WorldBuffer[i].PlayerCount = 0;
                worldOverview.WorldBuffer[i].ControllingFaction = Faction.LED;
                worldOverview.WorldBuffer[i].ControllingFactionRelation = FactionRelation.Neutral;
            }

            return worldOverview;
        }
    }
}
