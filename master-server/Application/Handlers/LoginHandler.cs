using FOMServer.Master.Application.Packets;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class LoginHandler : PacketHandlerBase<Login>
    {
        private readonly ILoginService _loginService;
        private readonly IWorldOverviewFactory _worldOverviewFactory;
        private readonly IClientPacketSender _packetSender;

        public LoginHandler(
            ILoginService loginService,
            IWorldOverviewFactory worldOverviewFactory,
            IClientPacketSender packetSender)
        {
            _loginService = loginService;
            _worldOverviewFactory = worldOverviewFactory;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in Login p)
        {
            var player = _loginService.Login(p.Username, p.PasswordHash, sender);
            if (player == null)
                return;

            using var response = new PacketWriter<LoginReturn>();
            ref var rData = ref response.Data;

            if (player.HasAvatar)
            {
                rData.Status = LoginReturn.StatusCode.LOGIN_RETURN_SUCCESS;
                rData.PlayerID = player.ID;
                rData.AccountType = 3;
                rData.IsVolunteer = false;
                rData.ClientVersion = GlobalConstants.ClientVersion;
                rData.WorldOverview = _worldOverviewFactory.Create(player);
            }
            else
                rData.Status = LoginReturn.StatusCode.LOGIN_RETURN_CREATE_CHARACTER;

            response.AddDestination(sender);
            _packetSender.Send(response.Build());
        }
    }
}
