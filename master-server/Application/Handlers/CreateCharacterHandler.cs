using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class CreateCharacterHandler : PacketHandlerBase<CreateCharacter>
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _packetSender;
        private readonly ILogger<CreateCharacterHandler> _logger;

        public CreateCharacterHandler(
            IPlayerRepository playerRepository,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            IClientPacketSender packetSender,
            ILogger<CreateCharacterHandler> logger)
        {
            _playerRepository = playerRepository;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _packetSender = packetSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in CreateCharacter p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                _logger.LogWarning("Dropping character creation from '{Address}' with no registered session", sender);
                return;
            }

            using var response = new PacketWriter<LoginReturn>(sender);
            ref var rData = ref response.Data;
            rData.PlayerId = p.PlayerId;

            if (session.Player is not null)
            {
                rData.Status = LoginReturn.StatusCode.Success;
                _packetSender.Send(response.Build());
                return;
            }

            var existing = _playerRepository.GetByName(p.Name);
            if (existing is not null)
            {
                rData.Status = LoginReturn.StatusCode.CreateCharacterError;
                _packetSender.Send(response.Build());
                return;
            }

            var created = _playerRepository.Create(
                p.PlayerId,
                p.Name,
                p.Biography,
                p.Avatar.Sex,
                p.Avatar.Race,
                p.Avatar.Face,
                p.Avatar.Hair);

            if (created is null)
            {
                rData.Status = LoginReturn.StatusCode.CreateCharacterError;
                _packetSender.Send(response.Build());
                return;
            }

            _clientRegistry.BeginLogin(session, p.PlayerId);
            _ = _playerRegistry.Login(session);

            rData.Status = LoginReturn.StatusCode.Success;
            _packetSender.Send(response.Build());
        }
    }
}
