using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Data;
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

        public override void Handle(NetworkAddress sender, in LoginRequest p)
        {
            using var response = QueuePacket.Create<LoginRequestReturn>();
            ref var rData = ref response.Data;

            unsafe
            {
                // We send back the username regardless of the outcome.
                for (int i = 0; i < LoginRequestReturn.UsernameSize; i++)
                    rData.RawUsername[i] = p.RawUsername[i];
            }

            var playerID = _playerRepository.Exists(p.Username);
            if (playerID == null)
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_INVALID_INFORMATION;
            else if (_playerService.Get(playerID.Value) != null)
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_ALREADY_LOGGED_IN;
            else if (p.ClientVersion != GlobalConstants.ClientVersion)
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_OUTDATED_CLIENT;
            else
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_SUCCESS;

            Console.WriteLine("Login attempt from {0} for user '{1}' - {2}",
                sender, p.Username, rData.Status);

            _packetSender.Send(response, sender, PacketPriority.Medium, PacketReliability.ReliableOrdered);
        }
    }
}
