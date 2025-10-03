using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;

namespace FOMServer.Master.Application.Handlers
{
    public class LoginRequestHandler : PacketHandler<LoginRequest>
    {
        public override PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN_REQUEST;

        private readonly IPlayerRepository playerRepository;
        private readonly IPlayerService playerService;
        private readonly IClientPacketSender packetSender;

        public LoginRequestHandler(
            IPlayerRepository playerRepository,
            IPlayerService playerService,
            IClientPacketSender packetSender
        )
        {
            this.playerService = playerService;
            this.playerRepository = playerRepository;
            this.packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in LoginRequest data)
        {
            var response = new LoginRequestReturn();
            unsafe
            {
                // We send back the username regardless of the outcome.
                for (int i = 0; i < 19; i++)
                    response.RawUsername[i] = data.RawUsername[i];
            }

            var playerID = playerRepository.Exists(data.Username);
            if (playerID == null)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_INVALID_INFORMATION;
            else if (playerService.Get(playerID.Value) != null)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_ALREADY_LOGGED_IN;
            else if (data.ClientVersion != GlobalConstants.ClientVersion)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_OUTDATED_CLIENT;
            else
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_SUCCESS;

            packetSender.Send(
                PacketIdentifier.ID_LOGIN_REQUEST_RETURN,
                new FOMDataUnion { loginRequestReturn = response },
                sender,
                PacketPriority.HIGH_PRIORITY,
                PacketReliability.RELIABLE_ORDERED
            );
        }
    }
}
