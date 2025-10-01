using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.FOMPacket.Data;

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

        public LoginReturn Create()
        {
            return new LoginReturn()
            {
                Status = LoginReturn.StatusCode.LOGIN_RETURN_CREATE_CHARACTER,
            };
        }
    }
}
