using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket.Data;
using static FOMServer.Shared.Core.FOMPacket.Data.LoginReturn;

namespace FOMServer.Master.Application.FOMPacket
{
    public class LoginReturnFactory : ILoginReturnFactory
    {
        private IPlayerService playerService;
        private IWorldServerService worldServerService;

        public LoginReturnFactory(IPlayerService playerService, IWorldServerService worldServerService)
        {
            this.playerService = playerService;
            this.worldServerService = worldServerService;
        }

        public LoginReturn Create(Player player)
        {
            if (!player.HasCharacter)
            {
                return new LoginReturn()
                {
                    Status = StatusCode.LOGIN_RETURN_CREATE_CHARACTER,
                };
            }

            var response = new LoginReturn()
            {
                Status = StatusCode.LOGIN_RETURN_SUCCESS,
                PlayerID = player.ID,
                AccountType = 3,
                IsVolunteer = false,
                ClientVersion = GlobalConstants.ClientVersion,
                OnlinePlayers = 0,
                OnlineNewPlayers = 0,
                IsPrisoner = false,
            };

            var worldServers = worldServerService.GetAll();
            response.NumWorlds = (byte)worldServers.Length;
            for (int i = 0; i < worldServers.Length; ++i)
            {
                var server = worldServers[i];

                response.WorldBuffer[i].ID = server.ID;
                response.WorldBuffer[i].Address = server.ClientAddress;
                response.WorldBuffer[i].PlayerCount = 0;
                response.WorldBuffer[i].ControllingFaction = Faction.LED;
                response.WorldBuffer[i].ControllingFactionRelation = FactionRelation.NEUTRAL;
            }

            return response;
        }
    }
}
