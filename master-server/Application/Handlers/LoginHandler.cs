using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class LoginHandler : PacketHandlerBase<Login>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _packetSender;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(
            IAccountRepository accountRepository,
            IPlayerRepository playerRepository,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            IClientPacketSender packetSender,
            ILogger<LoginHandler> logger)
        {
            _accountRepository = accountRepository;
            _playerRepository = playerRepository;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _packetSender = packetSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in Login p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                // A login with no registered session shouldn't happen (it implies a packet
                // arrived before NewIncomingConnection or after cleanup). Server->client packets
                // are feature-complete, so we can't signal this state; log and drop.
                _logger.LogWarning("Dropping login from '{Address}' with no registered session", sender);
                return;
            }

            using var response = new PacketWriter<LoginReturn>(sender);
            ref var rData = ref response.Data;

            // Already logged in on this session: idempotent success.
            if (session.Player is not null)
            {
                rData.PlayerID = session.Player.ID;
                rData.Status = LoginReturn.StatusCode.Success;
                rData.AccountType = AccountType.Prepaid;
                rData.LoginWorldID = WorldID.Manhattan;
                _packetSender.Send(response.Build());
                return;
            }

            var account = _accountRepository.GetByUsername(p.Username);
            if (account is null)
            {
                rData.Status = LoginReturn.StatusCode.InvalidLogin;
                _packetSender.Send(response.Build());
                return;
            }

            // Check Ban Status

            // Check Password

            rData.PlayerID = account.id;

            var player = _playerRepository.GetByID(account.id);
            if (player is null)
            {
                rData.Status = LoginReturn.StatusCode.CreateCharacter;
                _packetSender.Send(response.Build());
                return;
            }

            _clientRegistry.BeginLogin(session, account.id);
            _playerRegistry.Login(session);

            rData.Status = LoginReturn.StatusCode.Success;
            rData.AccountType = AccountType.Prepaid;
            rData.LoginWorldID = WorldID.Manhattan;
            _packetSender.Send(response.Build());
        }
    }
}
