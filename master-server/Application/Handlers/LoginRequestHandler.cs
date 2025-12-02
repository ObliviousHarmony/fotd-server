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
    public class LoginRequestHandler : PacketHandlerBase<LoginRequest>
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _packetSender;

        public LoginRequestHandler(
            IPlayerRepository playerRepository,
            IPlayerRegistry playerRegistry,
            IClientPacketSender packetSender
        )
        {
            _playerRepository = playerRepository;
            _playerRegistry = playerRegistry;
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in LoginRequest p)
        {
            using var response = new PacketWriter<LoginRequestReturn>();
            ref var rData = ref response.Data;

            unsafe
            {
                // We send back the username regardless of the outcome.
                for (int i = 0; i < LoginRequestReturn.UsernameSize; i++)
                    rData.RawUsername[i] = p.RawUsername[i];
            }

            var playerID = _playerRepository.GetIDByUsername(p.Username);
            if (playerID == null)
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_INVALID_INFORMATION;
            else if (_playerRegistry.Get(playerID.Value) != null)
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_ALREADY_LOGGED_IN;
            else if (p.ClientVersion != GlobalConstants.ClientVersion)
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_OUTDATED_CLIENT;
            else
                rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_SUCCESS;

            response.AddDestination(sender);
            _packetSender.Send(response.Build());
        }
    }
}
