using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Constants;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class LoginRequestPacketHandler : PacketHandlerBase<LoginRequestPacket>
    {
        private readonly IClientPacketSender _clientPacketSender;
        private readonly IAccountRepository _accountRepository;

        public LoginRequestPacketHandler(IClientPacketSender clientPacketSender, IAccountRepository accountRepository)
        {
            _clientPacketSender = clientPacketSender;
            _accountRepository = accountRepository;
        }

        public override void Handle(NetworkAddress sender, in LoginRequestPacket p)
        {
            using var response = new PacketWriter<LoginRequestReturnPacket>(sender);
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
            if (player is null)
            {
                rData.Status = LoginRequestReturnPacket.StatusCode.Invalid;
            }
            else if (player.logged_in)
            {
                rData.Status = LoginRequestReturnPacket.StatusCode.AlreadyLoggedIn;
            }
            else if (p.ClientVersion != ServerConstants.ClientVersion)
            {
                rData.Status = LoginRequestReturnPacket.StatusCode.VersionMismatch;
            }
            else
            {
                rData.Status = LoginRequestReturnPacket.StatusCode.Success;
            }

            _clientPacketSender.Send(response.Build());
        }
    }
}
