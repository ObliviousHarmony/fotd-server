using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.FOMPacket;
using FOMServer.Shared.Core.FOMPacket.Data;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class LoginRequestHandler : BasePacketHandler<LoginRequest>
    {
        public PacketIdentifier PacketID => PacketIdentifier.ID_LOGIN_REQUEST;

        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerService _playerService;
        private readonly IClientPacketSender _packetSender;

        public LoginRequestHandler(
            IPlayerRepository playerRepository,
            IPlayerService playerService,
            IClientPacketSender packetSender
        )
        {
            _playerService = playerService;
            _playerRepository = playerRepository;
            _packetSender = packetSender;
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

            var playerID = _playerRepository.Exists(data.Username);
            if (playerID == null)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_INVALID_INFORMATION;
            else if (_playerService.Get(playerID.Value) != null)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_ALREADY_LOGGED_IN;
            else if (data.ClientVersion != GlobalConstants.ClientVersion)
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_OUTDATED_CLIENT;
            else
                response.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_SUCCESS;

            _packetSender.Send(response, sender, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE_ORDERED);
        }
    }
}
