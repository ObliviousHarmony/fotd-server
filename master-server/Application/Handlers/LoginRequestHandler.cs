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
            using var response = new PacketBuilder<LoginRequestReturn>();
            ref var rData = ref response.Data;

            rData.Status = LoginRequestReturn.StatusCode.LOGIN_REQUEST_INVALID_INFORMATION;

            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            response.WithAddress(sender);
            _packetSender.Send(response.Build());
        }
    }
}
