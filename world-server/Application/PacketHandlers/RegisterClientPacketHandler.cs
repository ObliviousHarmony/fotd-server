using System.Diagnostics;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Application.World;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.PacketHandlers
{
    [PacketHandler]
    internal class RegisterClientPacketHandler : PacketHandlerBase<RegisterClientPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<RegisterClientPacketHandler> _logger;

        public RegisterClientPacketHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<RegisterClientPacketHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in RegisterClientPacket p)
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

            using var response = new PacketWriter<RegisterClientReturnPacket>(sender);
            ref var rData = ref response.Data;

            rData.WorldId = p.WorldId;
            rData.PlayerId = p.PlayerId;
            rData.Status = RegisterClientReturnPacket.StatusCode.Success;

            rData.NodeId = 1;

            player.WriteTo(ref rData);

            _clientPacketSender.Send(response.Build());

            using var worldObjects = new PacketWriter<WorldObjectsPacket>(sender);
            DummyWorldServices.WriteTo(ref worldObjects.Data);
            _clientPacketSender.Send(worldObjects.Build());
        }
    }
}
