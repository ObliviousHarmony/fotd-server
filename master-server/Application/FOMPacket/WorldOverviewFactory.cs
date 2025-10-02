using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Models;

namespace FOMServer.Master.Application.FOMPacket
{
    public class WorldOverviewFactory : IWorldOverviewFactory
    {
        private IWorldServerService worldServerService;

        public WorldOverviewFactory(IPlayerService playerService, IWorldServerService worldServerService)
        {
            this.worldServerService = worldServerService;
        }

        public WorldOverview Create(Player player)
        {
            var worldOverview = new WorldOverview()
            {
                OnlinePlayers = 0,
                OnlineNewPlayers = 0,
                IsPrisoner = false,
            };

            var worldServers = worldServerService.GetAll();
            worldOverview.NumWorlds = (byte)worldServers.Length;
            for (int i = 0; i < worldServers.Length; ++i)
            {
                var server = worldServers[i];

                worldOverview.WorldBuffer[i].ID = server.ID;
                worldOverview.WorldBuffer[i].Address = server.ClientAddress;
                worldOverview.WorldBuffer[i].PlayerCount = 0;
                worldOverview.WorldBuffer[i].ControllingFaction = Faction.LED;
                worldOverview.WorldBuffer[i].ControllingFactionRelation = FactionRelation.NEUTRAL;
            }

            return worldOverview;
        }
    }
}
