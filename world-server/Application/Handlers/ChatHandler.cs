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
    internal class ChatHandler : PacketHandlerBase<Chat>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<ChatHandler> _logger;

        public ChatHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<ChatHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in Chat p)
        {
            var player = _playerRegistry.Get(sender);
            if (player is null)
            {
                _logger.LogWarning("Received unexpected packet for player {PlayerId}", p.SenderId);
                return;
            }

            var responseMessage = "";

            var cmd = p.Message;
            if (cmd.StartsWith("!"))
            {
                cmd = cmd[1..];
                switch (cmd)
                {
                    case "pos":
                    {
                        responseMessage = $"Player {player.Id} ({player.Position})";
                        break;
                    }
                }
            }

            if (responseMessage.Length > 0)
            {
                using var response = new PacketWriter<Chat>(sender);
                ref var rData = ref response.Data;
                rData.Channel = p.Channel;
                rData.SenderId = p.SenderId;
                rData.SenderName = "Naruto Uzumaki";
                rData.Message = responseMessage;
                _clientPacketSender.Send(response.Build());
            }
        }
    }
}
