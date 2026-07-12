using FOMServer.Master.Application.Networking;
using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Handlers;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.Packets;
using FOMServer.Shared.Core.Packets.Types;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.Handlers
{
    [PacketHandler]
    internal class ChatHandler : PacketHandlerBase<Chat>
    {
        private readonly IClientRegistry _clientRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<ChatHandler> _logger;

        public ChatHandler(
            IClientRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<ChatHandler> logger
        )
        {
            _clientRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in Chat p)
        {
            var player = _clientRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected packet for player {PlayerId}", p.SenderId);
                return;
            }

            using var response = new PacketWriter<Chat>(sender);
            ref var rData = ref response.Data;
            rData.Channel = p.Channel;
            rData.SenderId = p.SenderId;
            rData.SenderName = "Naruto Uzumaki";
            rData.Message = p.Message;
            _clientPacketSender.Send(response.Build());
        }
    }
}
