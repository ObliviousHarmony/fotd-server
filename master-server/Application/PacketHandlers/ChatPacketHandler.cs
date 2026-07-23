using FOMServer.Master.Application.Networking;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class ChatPacketHandler : PacketHandlerBase<ChatPacket>
    {
        private readonly IClientRegistry _clientRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<ChatPacketHandler> _logger;

        public ChatPacketHandler(
            IClientRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<ChatPacketHandler> logger
        )
        {
            _clientRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in ChatPacket p)
        {
            var player = _clientRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected packet for player {PlayerId}", p.SenderId);
                return;
            }

            using var response = new PacketWriter<ChatPacket>(sender);
            ref var rData = ref response.Data;
            rData.Channel = p.Channel;
            rData.SenderId = p.SenderId;
            rData.SenderName = "Naruto Uzumaki";
            rData.Message = p.Message;
            _clientPacketSender.Send(response.Build());
        }
    }
}
