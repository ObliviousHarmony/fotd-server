using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Constants;
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
        private readonly IClientPacketSender _packetSender;
        private readonly IAccountRepository _accountRepository;
        private readonly IPlayerRepository _playerRepository;

        public LoginHandler(
            IClientPacketSender packetSender,
            IAccountRepository accountRepository,
            IPlayerRepository playerRepository)
        {
            _packetSender = packetSender;
            _accountRepository = accountRepository;
            _playerRepository = playerRepository;
        }

        public override void Handle(NetworkAddress sender, in Login p)
        {
            using var response = new PacketWriter<LoginReturn>(sender);
            ref var rData = ref response.Data;

            var account = _accountRepository.GetByUsername(p.Username);
            if (account == null)
            {
                rData.Status = LoginReturn.StatusCode.InvalidLogin;
                _packetSender.Send(response.Build());
                return;
            }

            // Check Ban Status

            // Check Password

            rData.PlayerID = account.id;

            var player = _playerRepository.GetByID(account.id);
            if (player == null)
            {
                rData.Status = LoginReturn.StatusCode.CreateCharacter;
                _packetSender.Send(response.Build());
                return;
            }

            rData.Status = LoginReturn.StatusCode.Success;

            // Populate Login Return
            rData.AccountType = AccountType.Prepaid;
            rData.LoginWorldID = WorldID.Manhattan;

            _packetSender.Send(response.Build());
        }
    }
}
