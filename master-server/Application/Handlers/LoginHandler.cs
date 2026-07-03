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
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(
            IAccountRepository accountRepository,
            IPlayerRepository playerRepository,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<LoginHandler> logger)
        {
            _accountRepository = accountRepository;
            _playerRepository = playerRepository;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in Login p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                _logger.LogWarning("Dropping login from '{Sender}' with no registered session", sender);
                return;
            }

            // DEV: temporary diagnostic — confirms the login reached this server.
            _logger.LogInformation("[DEV] Handling ID_LOGIN from {Sender} (hasPlayer={HasPlayer})", sender, session.Player is not null);

            using var response = new PacketWriter<LoginReturn>(sender);
            ref var rData = ref response.Data;

            if (session.Player is not null)
            {
                rData.PlayerId = session.Player.Id;
                rData.Status = LoginReturn.StatusCode.Success;

                // DEV: the client treats accountType as a numeric staff access level
                // (>= 22 = top tier), so 22 unlocks GM commands (not the bounded enum).
                // See knowledge-base/client/Account Access Levels.md.
                rData.AccountType = (AccountType)22;
                _logger.LogInformation("[DEV] LoginReturn Success (returning) player={PlayerId} accountType={AccountType}", rData.PlayerId, (byte)rData.AccountType);
                rData.LoginWorldId = WorldId.Manhattan;
                _clientPacketSender.Send(response.Build());
                return;
            }

            var account = _accountRepository.GetByUsername(p.Username);
            if (account is null)
            {
                rData.Status = LoginReturn.StatusCode.InvalidLogin;
                _clientPacketSender.Send(response.Build());
                return;
            }

            // Check Ban Status

            // Check Password

            rData.PlayerId = account.id;

            var player = _playerRepository.GetById(account.id);
            if (player is null)
            {
                rData.Status = LoginReturn.StatusCode.CreateCharacter;
                _clientPacketSender.Send(response.Build());
                return;
            }

            _clientRegistry.BeginLogin(session, account.id);
            _playerRegistry.Login(session);

            rData.Status = LoginReturn.StatusCode.Success;

            // DEV: the client treats accountType as a numeric staff access level
            // (>= 22 = top tier), so 22 unlocks GM commands (not the bounded enum).
            // See knowledge-base/client/Account Access Levels.md.
            rData.AccountType = (AccountType)22;
            _logger.LogInformation("[DEV] LoginReturn Success (fresh) player={PlayerId} accountType={AccountType}", rData.PlayerId, (byte)rData.AccountType);
            rData.LoginWorldId = WorldId.Manhattan;
            _clientPacketSender.Send(response.Build());
        }
    }
}
