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
        private readonly IClientPacketSender _packetSender;
        private readonly IAccountRepository _accountRepository;

        public LoginRequestHandler(
            IClientPacketSender packetSender,
            IAccountRepository accountRepository
        )
        {
            _packetSender = packetSender;
            _accountRepository = accountRepository;
        }

        public override void Handle(NetworkAddress sender, in LoginRequest p)
        {
            using var response = new PacketWriter<LoginRequestReturn>(sender);
            ref var rData = ref response.Data;

            unsafe
            {
                // We send back the username regardless of the outcome.
                for (int i = 0; i < BufferSizes.Username; i++)
                    rData.RawUsername[i] = p.RawUsername[i];
            }

            var player = _accountRepository.GetByUsername(p.Username);
            if (player is null)
                rData.Status = LoginRequestReturn.StatusCode.Invalid;
            else if (player.logged_in)
                rData.Status = LoginRequestReturn.StatusCode.AlreadyLoggedIn;
            else if (p.ClientVersion != ServerConstants.ClientVersion)
                rData.Status = LoginRequestReturn.StatusCode.VersionMismatch;
            else
                rData.Status = LoginRequestReturn.StatusCode.Success;

            _packetSender.Send(response.Build());
        }
    }
}
