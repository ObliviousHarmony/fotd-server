using FOMServer.Master.Core.Networking;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    public class LoginRequestHandler : PacketHandlerBase<LoginRequest>
    {
        private readonly IClientPacketSender _packetSender;

        public LoginRequestHandler(
            IClientPacketSender packetSender
        )
        {
            _packetSender = packetSender;
        }

        public override void Handle(NetworkAddress sender, in LoginRequest p)
        {
            using var response = new PacketWriter<LoginRequestReturn>();
            ref var rData = ref response.Data;

            unsafe
            {
                // We send back the username regardless of the outcome.
                for (int i = 0; i < LoginRequestReturn.UsernameSize; i++)
                    rData.RawUsername[i] = p.RawUsername[i];
            }

            /*

            var playerID = _playerRepository.GetIDByUsername(p.Username);
            if (playerID == null)
                rData.Status = LoginRequestReturn.StatusCode.Invalid;
            else if (_playerRegistry.Get(playerID.Value) != null)
                rData.Status = LoginRequestReturn.StatusCode.AlreadyLoggedIn;
            else if (p.ClientVersion != GlobalConstants.ClientVersion)
                rData.Status = LoginRequestReturn.StatusCode.VersionMismatch;
            else
                rData.Status = LoginRequestReturn.StatusCode.Success;

            */

            response.AddDestination(sender);
            _packetSender.Send(response.Build());
        }
    }
}
