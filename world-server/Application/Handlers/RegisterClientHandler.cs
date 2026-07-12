using System.Diagnostics;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.Handlers
{
    [PacketHandler]
    internal class RegisterClientHandler : PacketHandlerBase<RegisterClient>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<RegisterClientHandler> _logger;

        public RegisterClientHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<RegisterClientHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in RegisterClient p)
        {
            var player = _playerRegistry.ClaimForClient(p.PlayerId, sender);
            if (player is null)
            {
                _logger.LogWarning(
                    "Client '{Sender}' attempted to register unexpected player {PlayerId}",
                    sender,
                    p.PlayerId
                );
                return;
            }

            using var response = new PacketWriter<RegisterClientReturn>(sender);
            ref var rData = ref response.Data;

            rData.WorldId = p.WorldId;
            rData.PlayerId = p.PlayerId;
            rData.Status = RegisterClientReturn.StatusCode.Success;

            rData.NodeId = 1;

            player.WriteTo(ref rData);

            _clientPacketSender.Send(response.Build());
        }
    }
}
