using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class CreateCharacterPacketHandler : PacketHandlerBase<CreateCharacterPacket>
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<CreateCharacterPacketHandler> _logger;

        public CreateCharacterPacketHandler(
            IPlayerRepository playerRepository,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<CreateCharacterPacketHandler> logger
        )
        {
            _playerRepository = playerRepository;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in CreateCharacterPacket p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                _logger.LogWarning("Dropping character creation from '{Sender}' with no registered session", sender);
                return;
            }

            using var response = new PacketWriter<LoginReturnPacket>(sender);
            ref var rData = ref response.Data;
            rData.PlayerId = p.PlayerId;

            if (session.Player is not null)
            {
                rData.Status = LoginReturnPacket.StatusCode.Success;
                _clientPacketSender.Send(response.Build());
                return;
            }

            var existing = _playerRepository.GetByName(p.Name);
            if (existing is not null)
            {
                rData.Status = LoginReturnPacket.StatusCode.CreateCharacterError;
                _clientPacketSender.Send(response.Build());
                return;
            }

            var created = _playerRepository.Create(
                p.PlayerId,
                p.Name,
                p.Biography,
                p.Avatar.Sex,
                p.Avatar.Race,
                p.Avatar.Face,
                p.Avatar.Hair
            );

            if (created is null)
            {
                rData.Status = LoginReturnPacket.StatusCode.CreateCharacterError;
                _clientPacketSender.Send(response.Build());
                return;
            }

            _clientRegistry.BeginLogin(session, p.PlayerId);
            _playerRegistry.Login(session);

            rData.Status = LoginReturnPacket.StatusCode.Success;
            rData.AccountType = AccountType.Prepaid;
            rData.LoginWorldId = WorldId.Manhattan;
            _clientPacketSender.Send(response.Build());
        }
    }
}
