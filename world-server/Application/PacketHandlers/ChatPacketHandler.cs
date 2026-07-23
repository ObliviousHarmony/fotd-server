using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Metadata;
using FOMServer.World.Core.Networking;
using FOMServer.World.Core.Players;

namespace FOMServer.World.Application.PacketHandlers
{
    [PacketHandler]
    internal class ChatPacketHandler : PacketHandlerBase<ChatPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<ChatPacketHandler> _logger;

        public ChatPacketHandler(
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<ChatPacketHandler> logger
        )
        {
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in ChatPacket p)
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
                using var response = new PacketWriter<ChatPacket>(sender);
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
