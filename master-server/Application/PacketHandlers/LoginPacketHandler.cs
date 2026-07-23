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
    internal class LoginPacketHandler : PacketHandlerBase<LoginPacket>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<LoginPacketHandler> _logger;

        public LoginPacketHandler(
            IAccountRepository accountRepository,
            IPlayerRepository playerRepository,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<LoginPacketHandler> logger
        )
        {
            _accountRepository = accountRepository;
            _playerRepository = playerRepository;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in LoginPacket p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                _logger.LogWarning("Dropping login from '{Sender}' with no registered session", sender);
                return;
            }

            using var response = new PacketWriter<LoginReturnPacket>(sender);
            ref var rData = ref response.Data;

            if (session.Player is not null)
            {
                rData.PlayerId = session.Player.Id;
                rData.Status = LoginReturnPacket.StatusCode.Success;
                rData.AccountType = AccountType.Prepaid;
                rData.LoginWorldId = WorldId.Manhattan;
                _clientPacketSender.Send(response.Build());
                return;
            }

            var account = _accountRepository.GetByUsername(p.Username);
            if (account is null)
            {
                rData.Status = LoginReturnPacket.StatusCode.InvalidLogin;
                _clientPacketSender.Send(response.Build());
                return;
            }

            // Check Ban Status

            // Check Password

            rData.PlayerId = account.id;

            var player = _playerRepository.GetById(account.id);
            if (player is null)
            {
                rData.Status = LoginReturnPacket.StatusCode.CreateCharacter;
                _clientPacketSender.Send(response.Build());
                return;
            }

            _clientRegistry.BeginLogin(session, account.id);
            _playerRegistry.Login(session);

            rData.Status = LoginReturnPacket.StatusCode.Success;
            rData.AccountType = AccountType.Prepaid;
            rData.LoginWorldId = WorldId.Manhattan;
            _clientPacketSender.Send(response.Build());
        }
    }
}
