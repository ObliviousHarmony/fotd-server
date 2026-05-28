using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class LoginRequestHandler : PacketHandlerBase<LoginRequest>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IAccountRepository _accountRepository;

        public LoginRequestHandler(
            IClientPacketSender clientPacketSender,
            IAccountRepository accountRepository
        )
        {
            _clientPacketSender = clientPacketSender;
            _accountRepository = accountRepository;
        }

        public override void Handle(NetworkAddress sender, in LoginRequest p)
        {
            using var response = new PacketWriter<LoginRequestReturn>(sender);
            ref var rData = ref response.Data;

            unsafe
            {
                // We send back the username regardless of the outcome.
                for (var i = 0; i < BufferSizes.Username; i++)
                {
                    rData.RawUsername[i] = p.RawUsername[i];
                }
            }

            var player = _accountRepository.GetByUsername(p.Username);
            rData.Status = player is null
                ? LoginRequestReturn.StatusCode.Invalid
                : player.logged_in
                    ? LoginRequestReturn.StatusCode.AlreadyLoggedIn
                    : p.ClientVersion != ServerConstants.ClientVersion
                    ? LoginRequestReturn.StatusCode.VersionMismatch
                    : LoginRequestReturn.StatusCode.Success;

            _clientPacketSender.Send(response.Build());
        }
    }
}
